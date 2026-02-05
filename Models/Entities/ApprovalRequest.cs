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
        Cancelled = 3,

        [Display(Name = "На доработке")]
        Revision = 4,

        [Display(Name = "На юридической проверке")]
        LegalReview = 5,

        [Display(Name = "На рецензии")]
        UnderReview = 6,

        [Display(Name = "Делегировано")]
        Delegated = 7
    }

    /// <summary>
    /// Тип согласующего
    /// </summary>
    public enum ApproverType
    {
        [Display(Name = "Руководитель")]
        Manager = 0,

        [Display(Name = "Владелец документа")]
        DocumentOwner = 1,

        [Display(Name = "Рецензент")]
        Reviewer = 2,

        [Display(Name = "Юридический отдел")]
        LegalDepartment = 3,

        [Display(Name = "Должностное лицо по соблюдению требований")]
        ComplianceOfficer = 4,

        [Display(Name = "Заместитель")]
        Deputy = 5,

        [Display(Name = "Внешний согласующий")]
        External = 6
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

        /// <summary>
        /// Тип согласующего
        /// </summary>
        [Required]
        [Display(Name = "Тип согласующего")]
        public ApproverType ApproverType { get; set; } = ApproverType.Manager;

        /// <summary>
        /// Обязательное согласование (нельзя пропустить)
        /// </summary>
        [Display(Name = "Обязательное")]
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Может блокировать утверждение
        /// </summary>
        [Display(Name = "Может блокировать")]
        public bool CanBlock { get; set; } = false;

        /// <summary>
        /// ID пользователя, которому делегировано согласование
        /// </summary>
        public string? DelegatedToId { get; set; }

        /// <summary>
        /// Дата делегирования
        /// </summary>
        [Display(Name = "Дата делегирования")]
        public DateTime? DelegatedAt { get; set; }

        /// <summary>
        /// Причина делегирования
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Причина делегирования")]
        public string? DelegationReason { get; set; }

        /// <summary>
        /// Комментарий рецензента
        /// </summary>
        [Display(Name = "Комментарий рецензента")]
        public string? ReviewComment { get; set; }

        /// <summary>
        /// Предложенные правки
        /// </summary>
        [Display(Name = "Предложенные правки")]
        public string? SuggestedChanges { get; set; }

        // Навигационные свойства
        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        [ForeignKey("ApproverId")]
        public virtual ApplicationUser? Approver { get; set; }

        [ForeignKey("RequestedById")]
        public virtual ApplicationUser? RequestedBy { get; set; }

        [ForeignKey("DelegatedToId")]
        public virtual ApplicationUser? DelegatedTo { get; set; }
    }
}
