using System.ComponentModel.DataAnnotations;
using DocumentFlow.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace DocumentFlow.Models.ViewModels
{
    public class DocumentCreateViewModel
    {
        [Required(ErrorMessage = "Введите название документа")]
        [StringLength(500, ErrorMessage = "Название не должно превышать {1} символов")]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Выберите тип документа")]
        [Display(Name = "Тип документа")]
        public DocumentType DocumentType { get; set; }

        [Display(Name = "Шаблон")]
        public int? TemplateId { get; set; }

        [Display(Name = "Файл документа")]
        public IFormFile? File { get; set; }

        [Display(Name = "Содержимое")]
        public string? Content { get; set; }
    }

    public class DocumentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название документа")]
        [StringLength(500)]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Тип документа")]
        public DocumentType DocumentType { get; set; }

        [Display(Name = "Содержимое")]
        public string? Content { get; set; }

        [Display(Name = "Описание изменений")]
        public string? ChangeDescription { get; set; }

        [Display(Name = "Новый файл")]
        public IFormFile? NewFile { get; set; }

        public DocumentStatus Status { get; set; }
        public int CurrentVersion { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DocumentDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DocumentType DocumentType { get; set; }
        public DocumentStatus Status { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public int CurrentVersion { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? TemplateName { get; set; }

        public List<DocumentVersionViewModel> Versions { get; set; } = new();
        public List<ApprovalRequestViewModel> ApprovalRequests { get; set; } = new();
        public List<DocumentCommentViewModel> Comments { get; set; } = new();

        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSubmitForApproval { get; set; }
        public bool CanApprove { get; set; }
    }

    public class DocumentVersionViewModel
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ChangeDescription { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class DocumentListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; }
        public DocumentStatus Status { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int CurrentVersion { get; set; }
    }

    public class DocumentSearchViewModel
    {
        [Display(Name = "Поиск")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Тип документа")]
        public DocumentType? DocumentType { get; set; }

        [Display(Name = "Статус")]
        public DocumentStatus? Status { get; set; }

        [Display(Name = "Автор")]
        public string? AuthorId { get; set; }

        [Display(Name = "Дата с")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Дата по")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public List<DocumentListViewModel> Documents { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class SubmitForApprovalViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите согласующего")]
        [Display(Name = "Согласующий")]
        public List<string> ApproverIds { get; set; } = new();

        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }

        [Display(Name = "Срок согласования")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }

    // ViewModels для шаблонов
    public class TemplateCreateViewModel
    {
        [Required(ErrorMessage = "Введите название шаблона")]
        [StringLength(200, ErrorMessage = "Название не должно превышать {1} символов")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Выберите тип документа")]
        [Display(Name = "Тип документа")]
        public DocumentType DocumentType { get; set; }

        [Display(Name = "Содержимое шаблона")]
        public string? Content { get; set; }

        [Display(Name = "Файл шаблона")]
        public IFormFile? TemplateFile { get; set; }
    }

    public class TemplateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название шаблона")]
        [StringLength(200)]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Тип документа")]
        public DocumentType DocumentType { get; set; }

        [Display(Name = "Содержимое шаблона")]
        public string? Content { get; set; }

        [Display(Name = "Новый файл шаблона")]
        public IFormFile? TemplateFile { get; set; }

        public string? CurrentFilePath { get; set; }
        public bool IsActive { get; set; }
    }

    public class TemplateListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DocumentType DocumentType { get; set; }
        public bool IsActive { get; set; }
        public bool HasFile { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
