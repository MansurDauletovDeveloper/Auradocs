using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Версия документа для хранения истории изменений
    /// </summary>
    public class DocumentVersion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        [Display(Name = "Номер версии")]
        public int VersionNumber { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Имя файла")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Путь к файлу")]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "MIME-тип")]
        public string? ContentType { get; set; }

        [Display(Name = "Размер файла (байт)")]
        public long FileSize { get; set; }

        [Display(Name = "Хэш файла")]
        [StringLength(64)]
        public string? FileHash { get; set; }

        [Display(Name = "Содержимое (для текстовых документов)")]
        public string? Content { get; set; }

        [Display(Name = "Описание изменений")]
        public string? ChangeDescription { get; set; }

        [Required]
        public string CreatedById { get; set; } = string.Empty;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Текущая версия")]
        public bool IsCurrent { get; set; } = true;

        // Навигационные свойства
        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }
    }
}
