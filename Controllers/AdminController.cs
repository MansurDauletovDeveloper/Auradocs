using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace DocumentFlow.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IPasswordValidatorService _passwordValidatorService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService,
            IEmailVerificationService emailVerificationService,
            IPasswordValidatorService passwordValidatorService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _emailVerificationService = emailVerificationService;
            _passwordValidatorService = passwordValidatorService;
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

            // Валидация пароля
            var passwordValidation = _passwordValidatorService.Validate(model.Password);
            if (!passwordValidation.IsValid)
            {
                foreach (var error in passwordValidation.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
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
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false, // Требуется подтверждение email
                MustChangePassword = true // Требуется смена пароля при первом входе
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

                TempData["SuccessMessage"] = $"Пользователь {user.FullName} успешно создан. Временный пароль: {model.Password}. При первом входе пользователь должен подтвердить email и сменить пароль.";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Генерируем новый временный пароль
            var tempPassword = GenerateTemporaryPassword();

            // Сбрасываем пароль
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (result.Succeeded)
            {
                // Устанавливаем флаг обязательной смены пароля
                user.MustChangePassword = true;
                user.EmailConfirmed = false; // Требуем повторное подтверждение email
                await _userManager.UpdateAsync(user);

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await _auditService.LogAsync(currentUser.Id, AuditActionType.UserModified,
                        $"Сброшен пароль пользователя: {user.FullName}", entityName: "User", entityId: user.Id);
                }

                TempData["SuccessMessage"] = $"Пароль пользователя {user.FullName} сброшен. Новый временный пароль: {tempPassword}";
                return RedirectToAction(nameof(Users));
            }

            TempData["ErrorMessage"] = "Не удалось сбросить пароль: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Проверка, что пользователь не удаляет сам себя
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "Вы не можете удалить свою учётную запись.";
                return RedirectToAction(nameof(Users));
            }

            var userName = user.FullName;
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                if (currentUser != null)
                {
                    await _auditService.LogAsync(currentUser.Id, AuditActionType.UserModified,
                        $"Удалён пользователь: {userName}", entityName: "User", entityId: id);
                }

                TempData["SuccessMessage"] = $"Пользователь {userName} удалён.";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось удалить пользователя: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Генерирует безопасный временный пароль
        /// </summary>
        private static string GenerateTemporaryPassword()
        {
            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowercase = "abcdefghjkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%^&*";

            var password = new char[12];
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[12];
            rng.GetBytes(bytes);

            // Гарантируем наличие всех типов символов
            password[0] = uppercase[bytes[0] % uppercase.Length];
            password[1] = lowercase[bytes[1] % lowercase.Length];
            password[2] = digits[bytes[2] % digits.Length];
            password[3] = special[bytes[3] % special.Length];

            // Заполняем остальные символы случайными
            var allChars = uppercase + lowercase + digits + special;
            for (int i = 4; i < 12; i++)
            {
                password[i] = allChars[bytes[i] % allChars.Length];
            }

            // Перемешиваем
            return new string(password.OrderBy(x => Guid.NewGuid()).ToArray());
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
