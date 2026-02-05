using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DocumentFlow.Controllers
{
    /// <summary>
    /// Контроллер для IT-администраторов - управление правами и мониторинг сессий
    /// </summary>
    [Authorize(Roles = "Administrator,ITIntegrator")]
    public class ITAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ITAdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IAuditService auditService,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _auditService = auditService;
            _signInManager = signInManager;
        }

        #region Dashboard

        /// <summary>
        /// Главная панель IT администратора
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new ITAdminDashboardViewModel
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                TotalRoles = SystemRoles.AllRoles.Length,
                RecentLogins = await GetRecentLoginsAsync(10)
            };

            // Статистика по ролям
            foreach (var roleName in SystemRoles.AllRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                model.RoleStatistics.Add(new RoleStatisticsViewModel
                {
                    RoleName = roleName,
                    DisplayName = SystemRoles.GetDisplayName(roleName),
                    Description = SystemRoles.GetDescription(roleName),
                    UserCount = usersInRole.Count
                });
            }

            return View(model);
        }

        #endregion

        #region Users & Roles Management

        /// <summary>
        /// Список всех пользователей с их ролями
        /// </summary>
        public async Task<IActionResult> Users(string? searchTerm, string? roleFilter)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            var users = await usersQuery.ToListAsync();
            var model = new List<ITUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Фильтр по роли
                if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter))
                    continue;

                // Поиск по имени или email
                if (!string.IsNullOrEmpty(searchTerm) && 
                    !user.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) &&
                    !(user.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    continue;

                var lastLogin = await GetLastLoginForUserAsync(user.Id);

                model.Add(new ITUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    Position = user.Position,
                    Department = user.Department,
                    Roles = roles.ToList(),
                    RoleDisplayNames = roles.Select(r => SystemRoles.GetDisplayName(r)).ToList(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLogin = lastLogin,
                    CanExport = user.CanExport,
                    CanPrint = user.CanPrint,
                    CanDownload = user.CanDownload
                });
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.AvailableRoles = SystemRoles.AllRoles.Select(r => new SelectListItem
            {
                Value = r,
                Text = SystemRoles.GetDisplayName(r),
                Selected = r == roleFilter
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// Страница редактирования прав пользователя
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            
            var model = new EditUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName,
                Position = user.Position,
                Department = user.Department,
                IsActive = user.IsActive,
                CanExport = user.CanExport,
                CanPrint = user.CanPrint,
                CanDownload = user.CanDownload,
                SelectedRoles = userRoles.ToList(),
                AvailableRoles = SystemRoles.AllRoles.Select(r => new RoleOptionViewModel
                {
                    Name = r,
                    DisplayName = SystemRoles.GetDisplayName(r),
                    Description = SystemRoles.GetDescription(r),
                    IsSelected = userRoles.Contains(r)
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Сохранение прав пользователя
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            // Обновляем разрешения
            user.CanExport = model.CanExport;
            user.CanPrint = model.CanPrint;
            user.CanDownload = model.CanDownload;
            user.IsActive = model.IsActive;

            await _userManager.UpdateAsync(user);

            // Обновляем роли
            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.SelectedRoles ?? new List<string>();

            // Удаляем старые роли
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            // Добавляем новые роли
            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            // Логируем изменение
            var currentUserId = _userManager.GetUserId(User) ?? "";
            await _auditService.LogAsync(
                currentUserId,
                AuditActionType.UserModified,
                $"Роли пользователя {user.FullName} изменены IT-администратором. " +
                $"Удалено: {string.Join(", ", rolesToRemove)}. Добавлено: {string.Join(", ", rolesToAdd)}",
                entityName: "User",
                entityId: user.Id);

            TempData["SuccessMessage"] = $"Права пользователя {user.FullName} успешно обновлены";
            return RedirectToAction(nameof(Users));
        }

        #endregion

        #region Session Monitoring

        /// <summary>
        /// Мониторинг активных сессий / последних входов
        /// </summary>
        public async Task<IActionResult> Sessions()
        {
            var sessions = await GetRecentLoginsAsync(100);
            return View(sessions);
        }

        /// <summary>
        /// Журнал входов по пользователю
        /// </summary>
        public async Task<IActionResult> UserLoginHistory(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var loginHistory = await _context.AuditLogs
                .Where(a => a.UserId == id && a.ActionType == AuditActionType.Login)
                .OrderByDescending(a => a.Timestamp)
                .Take(50)
                .Select(a => new LoginHistoryViewModel
                {
                    Timestamp = a.Timestamp,
                    IpAddress = a.IpAddress,
                    Details = a.ActionDescription
                })
                .ToListAsync();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserLoginHistoryViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName,
                CurrentRoles = roles.Select(r => SystemRoles.GetDisplayName(r)).ToList(),
                LoginHistory = loginHistory
            };

            return View(model);
        }

        #endregion

        #region Roles Information

        /// <summary>
        /// Справочник всех ролей системы
        /// </summary>
        public IActionResult Roles()
        {
            var roles = SystemRoles.AllRoles.Select(r => new RoleInfoViewModel
            {
                Name = r,
                DisplayName = SystemRoles.GetDisplayName(r),
                Description = SystemRoles.GetDescription(r),
                CanApprove = SystemRoles.ApprovalRoles.Contains(r),
                CanComment = SystemRoles.CommentRoles.Contains(r),
                CanAccessAudit = SystemRoles.AuditAccessRoles.Contains(r),
                CanManageArchive = SystemRoles.ArchiveRoles.Contains(r)
            }).ToList();

            return View(roles);
        }

        #endregion

        #region Test Accounts

        /// <summary>
        /// Страница с тестовыми аккаунтами для всех ролей
        /// </summary>
        public async Task<IActionResult> TestAccounts()
        {
            var testAccounts = new List<TestAccountViewModel>();

            foreach (var role in SystemRoles.AllRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var testUser = usersInRole.FirstOrDefault();

                testAccounts.Add(new TestAccountViewModel
                {
                    RoleName = role,
                    RoleDisplayName = SystemRoles.GetDisplayName(role),
                    RoleDescription = SystemRoles.GetDescription(role),
                    Email = testUser?.Email ?? "Не создан",
                    Password = GetDefaultPasswordForRole(role),
                    Exists = testUser != null,
                    IsActive = testUser?.IsActive ?? false
                });
            }

            return View(testAccounts);
        }

        private string GetDefaultPasswordForRole(string role)
        {
            return role switch
            {
                SystemRoles.Administrator => "Admin123!",
                SystemRoles.Employee => "Employee123!",
                SystemRoles.Manager => "Manager123!",
                SystemRoles.DocumentOwner => "DocOwner123!",
                SystemRoles.Reviewer => "Reviewer123!",
                SystemRoles.LegalDepartment => "Legal123!",
                SystemRoles.Auditor => "Auditor123!",
                SystemRoles.Archivist => "Archivist123!",
                SystemRoles.ITIntegrator => "ITAdmin123!",
                SystemRoles.ExternalUser => "External123!",
                SystemRoles.Viewer => "Viewer123!",
                SystemRoles.ComplianceOfficer => "Compliance123!",
                SystemRoles.Deputy => "Deputy123!",
                _ => "Password123!"
            };
        }

        #endregion

        #region Quick Role Assignment

        /// <summary>
        /// Быстрое добавление роли пользователю
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (!SystemRoles.AllRoles.Contains(roleName))
                return BadRequest("Неверная роль");

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                var currentUserId = _userManager.GetUserId(User) ?? "";
                await _auditService.LogAsync(currentUserId, AuditActionType.UserModified,
                    $"Роль {SystemRoles.GetDisplayName(roleName)} добавлена пользователю {user.FullName}",
                    entityName: "User", entityId: userId);
                TempData["SuccessMessage"] = $"Роль {SystemRoles.GetDisplayName(roleName)} добавлена";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось добавить роль";
            }

            return RedirectToAction(nameof(EditRoles), new { id = userId });
        }

        /// <summary>
        /// Быстрое удаление роли у пользователя
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                var currentUserId = _userManager.GetUserId(User) ?? "";
                await _auditService.LogAsync(currentUserId, AuditActionType.UserModified,
                    $"Роль {SystemRoles.GetDisplayName(roleName)} удалена у пользователя {user.FullName}",
                    entityName: "User", entityId: userId);
                TempData["SuccessMessage"] = $"Роль {SystemRoles.GetDisplayName(roleName)} удалена";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось удалить роль";
            }

            return RedirectToAction(nameof(EditRoles), new { id = userId });
        }

        #endregion

        #region Helpers

        private async Task<List<RecentLoginViewModel>> GetRecentLoginsAsync(int count)
        {
            var logins = await _context.AuditLogs
                .Where(a => a.ActionType == AuditActionType.Login)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();

            var result = new List<RecentLoginViewModel>();

            foreach (var login in logins)
            {
                var user = await _userManager.FindByIdAsync(login.UserId ?? "");
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

                result.Add(new RecentLoginViewModel
                {
                    UserId = login.UserId ?? "",
                    UserName = user?.FullName ?? "Неизвестный",
                    Email = user?.Email ?? "",
                    Roles = roles.Select(r => SystemRoles.GetDisplayName(r)).ToList(),
                    LoginTime = login.Timestamp,
                    IpAddress = login.IpAddress
                });
            }

            return result;
        }

        private async Task<DateTime?> GetLastLoginForUserAsync(string userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId && a.ActionType == AuditActionType.Login)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => (DateTime?)a.Timestamp)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}
