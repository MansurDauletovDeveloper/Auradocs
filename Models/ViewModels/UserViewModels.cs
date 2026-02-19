using System.ComponentModel.DataAnnotations;
using DocumentFlow.Models.Entities;

namespace DocumentFlow.Models.ViewModels
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsExternalUser { get; set; }
        public DateTime? AccessExpiresAt { get; set; }
        public string? ManagerName { get; set; }
    }

    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        [Display(Name = "Должность")]
        public string? Position { get; set; }

        [Display(Name = "Отдел")]
        public string? Department { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Роли")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<RoleViewModel> AvailableRoles { get; set; } = new();

        [Display(Name = "Руководитель")]
        public string? ManagerId { get; set; }

        [Display(Name = "Разрешён экспорт")]
        public bool CanExport { get; set; } = true;

        [Display(Name = "Разрешена печать")]
        public bool CanPrint { get; set; } = true;

        [Display(Name = "Разрешено скачивание")]
        public bool CanDownload { get; set; } = true;

        [Display(Name = "Внешний пользователь")]
        public bool IsExternalUser { get; set; }

        [Display(Name = "Доступ до")]
        [DataType(DataType.Date)]
        public DateTime? AccessExpiresAt { get; set; }

        public List<UserSelectViewModel> AvailableManagers { get; set; } = new();
    }

    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        [Display(Name = "Должность")]
        public string? Position { get; set; }

        [Display(Name = "Отдел")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(128, ErrorMessage = "Пароль должен содержать от {2} до {1} символов", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Временный пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Роли")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<RoleViewModel> AvailableRoles { get; set; } = new();

        [Display(Name = "Руководитель")]
        public string? ManagerId { get; set; }

        [Display(Name = "Разрешён экспорт")]
        public bool CanExport { get; set; } = true;

        [Display(Name = "Разрешена печать")]
        public bool CanPrint { get; set; } = true;

        [Display(Name = "Разрешено скачивание")]
        public bool CanDownload { get; set; } = true;

        [Display(Name = "Внешний пользователь")]
        public bool IsExternalUser { get; set; }

        [Display(Name = "Доступ до")]
        [DataType(DataType.Date)]
        public DateTime? AccessExpiresAt { get; set; }

        public List<UserSelectViewModel> AvailableManagers { get; set; } = new();
    }

    public class RoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class UserSelectViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
    }

    /// <summary>
    /// ViewModel для делегирования полномочий
    /// </summary>
    public class DelegationCreateViewModel
    {
        [Required(ErrorMessage = "Выберите пользователя")]
        [Display(Name = "Делегировать кому")]
        public string ToUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите дату начала")]
        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Укажите дату окончания")]
        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Причина")]
        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        [Display(Name = "Тип делегирования")]
        public DelegationType DelegationType { get; set; } = DelegationType.Full;

        public List<UserSelectViewModel> AvailableUsers { get; set; } = new();
    }

    public class DelegationListViewModel
    {
        public int Id { get; set; }
        public string FromUserName { get; set; } = string.Empty;
        public string ToUserName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public DelegationType DelegationType { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }

    /// <summary>
    /// ViewModel для приглашения внешнего пользователя
    /// </summary>
    public class ExternalUserInviteViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Организация")]
        public string? Organization { get; set; }

        [Required(ErrorMessage = "Укажите срок доступа")]
        [Display(Name = "Доступ до")]
        [DataType(DataType.Date)]
        public DateTime AccessExpiresAt { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "Разрешить скачивание")]
        public bool CanDownload { get; set; } = false;

        [Display(Name = "Разрешить печать")]
        public bool CanPrint { get; set; } = false;

        [Display(Name = "Разрешить экспорт")]
        public bool CanExport { get; set; } = false;

        [Display(Name = "Документы для доступа")]
        public List<int> DocumentIds { get; set; } = new();

        [Display(Name = "Уровень доступа")]
        public AccessLevel AccessLevel { get; set; } = AccessLevel.View;
    }
}
