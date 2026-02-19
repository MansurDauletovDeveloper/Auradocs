using System.ComponentModel.DataAnnotations;

namespace DocumentFlow.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// Модель для подтверждения email кодом
    /// </summary>
    public class VerifyEmailViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите код подтверждения")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен содержать 6 цифр")]
        [Display(Name = "Код подтверждения")]
        public string Code { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
        
        public int? SecondsToResend { get; set; }
    }

    /// <summary>
    /// Модель для принудительной смены пароля
    /// </summary>
    public class ForceChangePasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(128, ErrorMessage = "Пароль должен содержать от {2} до {1} символов", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// Результат проверки силы пароля (для клиентской валидации)
    /// </summary>
    public class PasswordStrengthResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public int StrengthScore { get; set; }
        public string StrengthLevel { get; set; } = string.Empty;
    }

    // RegisterViewModel удалён - самостоятельная регистрация отключена
    // Пользователи создаются только администратором

    public class UserProfileViewModel
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

        [Display(Name = "Телефон")]
        [Phone]
        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введите текущий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(128, ErrorMessage = "Пароль должен содержать от {2} до {1} символов", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
