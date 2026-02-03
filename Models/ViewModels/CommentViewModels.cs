using System.ComponentModel.DataAnnotations;

namespace DocumentFlow.Models.ViewModels
{
    public class DocumentCommentViewModel
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public List<DocumentCommentViewModel> Replies { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class AddCommentViewModel
    {
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Введите текст комментария")]
        [Display(Name = "Комментарий")]
        public string Text { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
    }
}
