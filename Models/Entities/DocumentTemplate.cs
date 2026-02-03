using System.ComponentModel.DataAnnotations;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Шаблон документа
    /// </summary>
    public class DocumentTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Тип документа")]
        public DocumentType DocumentType { get; set; }

        [StringLength(500)]
        [Display(Name = "Путь к файлу шаблона")]
        public string? TemplatePath { get; set; }

        [Display(Name = "Содержимое шаблона")]
        public string? Content { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
