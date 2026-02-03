using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Статус запроса на согласование
    /// </summary>
    public enum ApprovalStatus
    {
        [Display(Name = "Ожидает")]
        Pending = 0,

        [Display(Name = "Утверждён")]
        Approved = 1,

        [Display(Name = "Отклонён")]
        Rejected = 2,

        [Display(Name = "Отменён")]
        Cancelled = 3
    }

    /// <summary>
    /// Запрос на согласование документа
    /// </summary>
    public class ApprovalRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        public string ApproverId { get; set; } = string.Empty;

        [Required]
        public string RequestedById { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Статус")]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Display(Name = "Порядок согласования")]
        public int Order { get; set; } = 1;

        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата обработки")]
        public DateTime? ProcessedAt { get; set; }

        [Display(Name = "Срок согласования")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Уведомление отправлено")]
        public bool NotificationSent { get; set; } = false;

        // Навигационные свойства
        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        [ForeignKey("ApproverId")]
        public virtual ApplicationUser? Approver { get; set; }

        [ForeignKey("RequestedById")]
        public virtual ApplicationUser? RequestedBy { get; set; }
    }
}
