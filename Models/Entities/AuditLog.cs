using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Тип действия для журнала аудита
    /// </summary>
    public enum AuditActionType
    {
        [Display(Name = "Создание")]
        Create = 0,

        [Display(Name = "Просмотр")]
        View = 1,

        [Display(Name = "Редактирование")]
        Edit = 2,

        [Display(Name = "Удаление")]
        Delete = 3,

        [Display(Name = "Отправка на согласование")]
        SubmitForApproval = 4,

        [Display(Name = "Утверждение")]
        Approve = 5,

        [Display(Name = "Отклонение")]
        Reject = 6,

        [Display(Name = "Скачивание")]
        Download = 7,

        [Display(Name = "Загрузка версии")]
        UploadVersion = 8,

        [Display(Name = "Восстановление версии")]
        RestoreVersion = 9,

        [Display(Name = "Добавление комментария")]
        AddComment = 10,

        [Display(Name = "Вход в систему")]
        Login = 11,

        [Display(Name = "Выход из системы")]
        Logout = 12,

        [Display(Name = "Изменение пользователя")]
        UserModified = 13,

        [Display(Name = "Архивирование")]
        Archive = 14
    }

    /// <summary>
    /// Журнал действий (Audit Log)
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Тип действия")]
        public AuditActionType ActionType { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Описание действия")]
        public string ActionDescription { get; set; } = string.Empty;

        public int? DocumentId { get; set; }

        [StringLength(100)]
        [Display(Name = "Название сущности")]
        public string? EntityName { get; set; }

        [Display(Name = "ID сущности")]
        public string? EntityId { get; set; }

        [Display(Name = "Старое значение")]
        public string? OldValue { get; set; }

        [Display(Name = "Новое значение")]
        public string? NewValue { get; set; }

        [StringLength(50)]
        [Display(Name = "IP-адрес")]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        [Display(Name = "User Agent")]
        public string? UserAgent { get; set; }

        [Display(Name = "Дата и время")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }
    }
}
