using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DocumentFlow.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
        }

        #region Users Management

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    Position = user.Position,
                    Department = user.Department,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var model = new UserCreateViewModel
            {
                AvailableRoles = await GetRolesAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await GetRolesAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MiddleName = model.MiddleName,
                Position = model.Position,
                Department = model.Department,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Employee");
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await _auditService.LogAsync(currentUser.Id, AuditActionType.UserModified,
                        $"Создан пользователь: {user.FullName}", entityName: "User", entityId: user.Id);
                }

                TempData["SuccessMessage"] = "Пользователь успешно создан.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableRoles = await GetRolesAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Position = user.Position,
                Department = user.Department,
                IsActive = user.IsActive,
                SelectedRoles = userRoles.ToList(),
                AvailableRoles = await GetRolesAsync(userRoles)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await GetRolesAsync(model.SelectedRoles);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.MiddleName = model.MiddleName;
            user.Position = model.Position;
            user.Department = model.Department;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Обновляем роли
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await _auditService.LogAsync(currentUser.Id, AuditActionType.UserModified,
                        $"Изменён пользователь: {user.FullName}", entityName: "User", entityId: user.Id);
                }

                TempData["SuccessMessage"] = "Пользователь успешно обновлён.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableRoles = await GetRolesAsync(model.SelectedRoles);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                await _auditService.LogAsync(currentUser.Id, AuditActionType.UserModified,
                    $"Статус пользователя {user.FullName} изменён на {(user.IsActive ? "активен" : "заблокирован")}",
                    entityName: "User", entityId: user.Id);
            }

            TempData["SuccessMessage"] = user.IsActive 
                ? "Пользователь активирован." 
                : "Пользователь заблокирован.";
            
            return RedirectToAction(nameof(Users));
        }

        #endregion

        #region Audit Logs

        public async Task<IActionResult> AuditLogs(AuditLogSearchViewModel search)
        {
            var (logs, totalCount) = await _auditService.GetLogsAsync(
                search.UserId, search.ActionType, search.DocumentId,
                search.DateFrom, search.DateTo, search.Page, search.PageSize);

            search.Logs = logs.Select(l => new AuditLogViewModel
            {
                Id = l.Id,
                UserName = l.User?.FullName ?? "Неизвестно",
                UserId = l.UserId,
                ActionType = l.ActionType,
                ActionDescription = l.ActionDescription,
                DocumentId = l.DocumentId,
                DocumentTitle = l.Document?.Title,
                IpAddress = l.IpAddress,
                Timestamp = l.Timestamp
            }).ToList();

            search.TotalCount = totalCount;

            // Заполняем фильтры
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = users.Select(u => new SelectListItem(u.FullName, u.Id)).ToList();

            return View(search);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAuditLogs(AuditLogSearchViewModel search)
        {
            search.PageSize = 100000;
            var (logs, _) = await _auditService.GetLogsAsync(
                search.UserId, search.ActionType, search.DocumentId,
                search.DateFrom, search.DateTo, 1, search.PageSize);

            var csv = new StringBuilder();
            csv.AppendLine("Дата;Пользователь;Действие;Описание;Документ;IP-адрес");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Timestamp:dd.MM.yyyy HH:mm:ss};{log.User?.FullName};{GetActionTypeName(log.ActionType)};{log.ActionDescription};{log.Document?.Title};{log.IpAddress}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"audit_logs_{DateTime.Now:yyyyMMdd}.csv");
        }

        #endregion

        private async Task<List<RoleViewModel>> GetRolesAsync(IList<string>? selectedRoles = null)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name ?? "",
                DisplayName = GetRoleDisplayName(r.Name),
                IsSelected = selectedRoles?.Contains(r.Name ?? "") ?? false
            }).ToList();
        }

        private static string GetRoleDisplayName(string? roleName) => roleName switch
        {
            "Administrator" => "Администратор",
            "Manager" => "Руководитель",
            "Employee" => "Сотрудник",
            _ => roleName ?? ""
        };

        private static string GetActionTypeName(AuditActionType type) => type switch
        {
            AuditActionType.Create => "Создание",
            AuditActionType.View => "Просмотр",
            AuditActionType.Edit => "Редактирование",
            AuditActionType.Delete => "Удаление",
            AuditActionType.SubmitForApproval => "Отправка на согласование",
            AuditActionType.Approve => "Утверждение",
            AuditActionType.Reject => "Отклонение",
            AuditActionType.Download => "Скачивание",
            AuditActionType.UploadVersion => "Загрузка версии",
            AuditActionType.RestoreVersion => "Восстановление версии",
            AuditActionType.AddComment => "Комментарий",
            AuditActionType.Login => "Вход",
            AuditActionType.Logout => "Выход",
            AuditActionType.UserModified => "Изменение пользователя",
            _ => "Неизвестно"
        };
    }
}
