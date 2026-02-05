using System.ComponentModel.DataAnnotations;

namespace DocumentFlow.Models.ViewModels
{
    /// <summary>
    /// Модель главной панели IT-администратора
    /// </summary>
    public class ITAdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }
        public List<RoleStatisticsViewModel> RoleStatistics { get; set; } = new();
        public List<RecentLoginViewModel> RecentLogins { get; set; } = new();
    }

    /// <summary>
    /// Статистика по роли
    /// </summary>
    public class RoleStatisticsViewModel
    {
        public string RoleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    /// <summary>
    /// Информация о входе пользователя
    /// </summary>
    public class RecentLoginViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime LoginTime { get; set; }
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// Расширенная модель пользователя для IT-администратора
    /// </summary>
    public class ITUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> RoleDisplayNames { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool CanExport { get; set; }
        public bool CanPrint { get; set; }
        public bool CanDownload { get; set; }
    }

    /// <summary>
    /// Модель редактирования ролей пользователя
    /// </summary>
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
        
        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Разрешён экспорт")]
        public bool CanExport { get; set; }

        [Display(Name = "Разрешена печать")]
        public bool CanPrint { get; set; }

        [Display(Name = "Разрешено скачивание")]
        public bool CanDownload { get; set; }

        public List<string> SelectedRoles { get; set; } = new();
        public List<RoleOptionViewModel> AvailableRoles { get; set; } = new();
    }

    /// <summary>
    /// Опция роли для выбора
    /// </summary>
    public class RoleOptionViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Информация о роли системы
    /// </summary>
    public class RoleInfoViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool CanApprove { get; set; }
        public bool CanComment { get; set; }
        public bool CanAccessAudit { get; set; }
        public bool CanManageArchive { get; set; }
    }

    /// <summary>
    /// Тестовый аккаунт
    /// </summary>
    public class TestAccountViewModel
    {
        public string RoleName { get; set; } = string.Empty;
        public string RoleDisplayName { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// История входов пользователя
    /// </summary>
    public class UserLoginHistoryViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> CurrentRoles { get; set; } = new();
        public List<LoginHistoryViewModel> LoginHistory { get; set; } = new();
    }

    /// <summary>
    /// Запись истории входа
    /// </summary>
    public class LoginHistoryViewModel
    {
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
    }
}
