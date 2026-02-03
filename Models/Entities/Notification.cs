using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Тип уведомления
    /// </summary>
    public enum NotificationType
    {
        [Display(Name = "Документ на согласование")]
        ApprovalRequest = 0,

        [Display(Name = "Документ утверждён")]
        DocumentApproved = 1,

        [Display(Name = "Документ отклонён")]
        DocumentRejected = 2,

        [Display(Name = "Комментарий")]
        Comment = 3,

        [Display(Name = "Напоминание")]
        Reminder = 4,

        [Display(Name = "Системное")]
        System = 5
    }

    /// <summary>
    /// Уведомление пользователя
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Сообщение")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Тип")]
        public NotificationType Type { get; set; }

        public int? DocumentId { get; set; }

        [Display(Name = "Прочитано")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Email отправлен")]
        public bool EmailSent { get; set; } = false;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата прочтения")]
        public DateTime? ReadAt { get; set; }

        // Навигационные свойства
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }
    }
}
