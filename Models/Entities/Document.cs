using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Статус документа
    /// </summary>
    public enum DocumentStatus
    {
        [Display(Name = "Черновик")]
        Draft = 0,

        [Display(Name = "На согласовании")]
        Pending = 1,

        [Display(Name = "Утверждён")]
        Approved = 2,

        [Display(Name = "Отклонён")]
        Rejected = 3,

        [Display(Name = "Архив")]
        Archived = 4
    }

    /// <summary>
    /// Тип документа
    /// </summary>
    public enum DocumentType
    {
        [Display(Name = "Заявление")]
        Application = 0,

        [Display(Name = "Приказ")]
        Order = 1,

        [Display(Name = "Договор")]
        Contract = 2,

        [Display(Name = "Акт")]
        Act = 3,

        [Display(Name = "Служебная записка")]
        Memo = 4,

        [Display(Name = "Отчёт")]
        Report = 5,

        [Display(Name = "Протокол")]
        Protocol = 6,

        [Display(Name = "Другое")]
        Other = 7
    }

    /// <summary>
    /// Документ в системе электронного документооборота
    /// </summary>
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Тип документа")]
        public DocumentType DocumentType { get; set; }

        [Required]
        [Display(Name = "Статус")]
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        [Required]
        [StringLength(100)]
        [Display(Name = "Регистрационный номер")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата изменения")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Текущая версия")]
        public int CurrentVersion { get; set; } = 1;

        // Внешние ключи
        [Required]
        public string AuthorId { get; set; } = string.Empty;

        public int? TemplateId { get; set; }

        // Навигационные свойства
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser? Author { get; set; }

        [ForeignKey("TemplateId")]
        public virtual DocumentTemplate? Template { get; set; }

        public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
        public virtual ICollection<ApprovalRequest> ApprovalRequests { get; set; } = new List<ApprovalRequest>();
        public virtual ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
