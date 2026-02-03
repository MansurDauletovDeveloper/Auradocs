using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Комментарий к документу
    /// </summary>
    public class DocumentComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Текст комментария")]
        public string Text { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата изменения")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Удалён")]
        public bool IsDeleted { get; set; } = false;

        // Навигационные свойства
        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        [ForeignKey("AuthorId")]
        public virtual ApplicationUser? Author { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual DocumentComment? ParentComment { get; set; }

        public virtual ICollection<DocumentComment> Replies { get; set; } = new List<DocumentComment>();
    }
}
