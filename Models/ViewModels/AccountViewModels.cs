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

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(100, ErrorMessage = "Имя должно содержать от {2} до {1} символов", MinimumLength = 2)]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(100, ErrorMessage = "Фамилия должна содержать от {2} до {1} символов", MinimumLength = 2)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        [StringLength(200)]
        [Display(Name = "Должность")]
        public string? Position { get; set; }

        [StringLength(200)]
        [Display(Name = "Отдел")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

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
        [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
