using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Пользователь системы с расширенными свойствами
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
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

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ID руководителя (для иерархии подчинения)
        /// </summary>
        [Display(Name = "Руководитель")]
        public string? ManagerId { get; set; }

        /// <summary>
        /// Может ли экспортировать документы
        /// </summary>
        [Display(Name = "Разрешён экспорт")]
        public bool CanExport { get; set; } = true;

        /// <summary>
        /// Может ли печатать документы
        /// </summary>
        [Display(Name = "Разрешена печать")]
        public bool CanPrint { get; set; } = true;

        /// <summary>
        /// Может ли скачивать документы
        /// </summary>
        [Display(Name = "Разрешено скачивание")]
        public bool CanDownload { get; set; } = true;

        /// <summary>
        /// Внешний пользователь (ограниченный доступ)
        /// </summary>
        [Display(Name = "Внешний пользователь")]
        public bool IsExternalUser { get; set; } = false;

        /// <summary>
        /// Дата окончания доступа для внешних пользователей
        /// </summary>
        [Display(Name = "Доступ до")]
        public DateTime? AccessExpiresAt { get; set; }

        /// <summary>
        /// Последний вход в систему
        /// </summary>
        [Display(Name = "Последний вход")]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "Полное имя")]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        // Навигационные свойства
        [ForeignKey("ManagerId")]
        public virtual ApplicationUser? Manager { get; set; }

        public virtual ICollection<ApplicationUser> Subordinates { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<Document> CreatedDocuments { get; set; } = new List<Document>();
        public virtual ICollection<Document> OwnedDocuments { get; set; } = new List<Document>();
        public virtual ICollection<ApprovalRequest> ApprovalRequests { get; set; } = new List<ApprovalRequest>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();
        public virtual ICollection<UserDelegation> DelegationsFrom { get; set; } = new List<UserDelegation>();
        public virtual ICollection<UserDelegation> DelegationsTo { get; set; } = new List<UserDelegation>();
        public virtual ICollection<DocumentAccess> DocumentAccesses { get; set; } = new List<DocumentAccess>();
    }
}
