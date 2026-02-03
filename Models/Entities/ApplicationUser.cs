using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Полное имя")]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        // Навигационные свойства
        public virtual ICollection<Document> CreatedDocuments { get; set; } = new List<Document>();
        public virtual ICollection<ApprovalRequest> ApprovalRequests { get; set; } = new List<ApprovalRequest>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();
    }
}
