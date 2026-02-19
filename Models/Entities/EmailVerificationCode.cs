using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Тип верификации
    /// </summary>
    public enum VerificationType
    {
        [Display(Name = "Подтверждение email")]
        EmailConfirmation = 0,

        [Display(Name = "Сброс пароля")]
        PasswordReset = 1,

        [Display(Name = "Двухфакторная аутентификация")]
        TwoFactorAuth = 2
    }

    /// <summary>
    /// Код верификации email
    /// </summary>
    public class EmailVerificationCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        [Display(Name = "Код верификации")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Тип верификации")]
        public VerificationType Type { get; set; } = VerificationType.EmailConfirmation;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Срок действия")]
        public DateTime ExpiresAt { get; set; }

        [Display(Name = "Использован")]
        public bool IsUsed { get; set; } = false;

        [Display(Name = "Дата использования")]
        public DateTime? UsedAt { get; set; }

        [Display(Name = "Количество попыток")]
        public int AttemptCount { get; set; } = 0;

        [Display(Name = "Максимум попыток")]
        public int MaxAttempts { get; set; } = 5;

        [Display(Name = "IP адрес запроса")]
        [StringLength(50)]
        public string? RequestIpAddress { get; set; }

        // Навигационное свойство
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// Проверяет, истёк ли срок действия кода
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        /// <summary>
        /// Проверяет, превышено ли количество попыток
        /// </summary>
        public bool IsMaxAttemptsReached => AttemptCount >= MaxAttempts;

        /// <summary>
        /// Проверяет, валиден ли код для использования
        /// </summary>
        public bool IsValid => !IsUsed && !IsExpired && !IsMaxAttemptsReached;
    }

    /// <summary>
    /// История входов пользователя
    /// </summary>
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Время входа")]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        [Display(Name = "IP адрес")]
        [StringLength(50)]
        public string? IpAddress { get; set; }

        [Display(Name = "User Agent")]
        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Display(Name = "Успешный вход")]
        public bool IsSuccessful { get; set; }

        [Display(Name = "Причина неудачи")]
        [StringLength(500)]
        public string? FailureReason { get; set; }

        [Display(Name = "Местоположение")]
        [StringLength(200)]
        public string? Location { get; set; }

        // Навигационное свойство
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
