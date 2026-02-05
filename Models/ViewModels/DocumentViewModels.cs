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

        [Display(Name = "Конфиденциальность")]
        public ConfidentialityLevel ConfidentialityLevel { get; set; } = ConfidentialityLevel.Internal;

        [Display(Name = "Требуется юридическая проверка")]
        public bool RequiresLegalReview { get; set; } = false;

        [Display(Name = "Владелец документа")]
        public string? OwnerId { get; set; }

        public List<UserSelectViewModel> AvailableOwners { get; set; } = new();
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

        [Display(Name = "Конфиденциальность")]
        public ConfidentialityLevel ConfidentialityLevel { get; set; }

        [Display(Name = "Требуется юридическая проверка")]
        public bool RequiresLegalReview { get; set; }

        [Display(Name = "Владелец документа")]
        public string? OwnerId { get; set; }

        public List<UserSelectViewModel> AvailableOwners { get; set; } = new();
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

        // Новые поля
        public ConfidentialityLevel ConfidentialityLevel { get; set; }
        public bool RequiresLegalReview { get; set; }
        public bool LegalReviewCompleted { get; set; }
        public string? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public int? RetentionPeriodMonths { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }

        public List<DocumentVersionViewModel> Versions { get; set; } = new();
        public List<ApprovalRequestViewModel> ApprovalRequests { get; set; } = new();
        public List<DocumentCommentViewModel> Comments { get; set; } = new();
        public List<DocumentAccessViewModel> AccessList { get; set; } = new();
        public List<DocumentBlockViewModel> Blocks { get; set; } = new();

        // Права доступа
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSubmitForApproval { get; set; }
        public bool CanApprove { get; set; }
        public bool CanComment { get; set; }
        public bool CanDownload { get; set; }
        public bool CanPrint { get; set; }
        public bool CanExport { get; set; }
        public bool CanBlock { get; set; }
        public bool CanUnblock { get; set; }
        public bool CanArchive { get; set; }
        public bool CanRestore { get; set; }
        public bool CanManageAccess { get; set; }
        public bool CanDelegate { get; set; }
        public bool CanRequestRevision { get; set; }
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
        public ConfidentialityLevel ConfidentialityLevel { get; set; }
        public bool IsBlocked { get; set; }
        public string? OwnerName { get; set; }
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

        [Display(Name = "Конфиденциальность")]
        public ConfidentialityLevel? ConfidentialityLevel { get; set; }

        [Display(Name = "Владелец")]
        public string? OwnerId { get; set; }

        [Display(Name = "Отдел")]
        public string? Department { get; set; }

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

    /// <summary>
    /// ViewModel для доступа к документу
    /// </summary>
    public class DocumentAccessViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserPosition { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public string? DocumentRole { get; set; }
        public bool CanDownload { get; set; }
        public bool CanPrint { get; set; }
        public bool CanExport { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? GrantedByName { get; set; }
        public DateTime GrantedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }

    /// <summary>
    /// ViewModel для блокировки документа
    /// </summary>
    public class DocumentBlockViewModel
    {
        public int Id { get; set; }
        public string BlockedByName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public BlockType BlockType { get; set; }
        public DateTime BlockedAt { get; set; }
        public DateTime? UnblockedAt { get; set; }
        public string? UnblockedByName { get; set; }
        public string? UnblockComment { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel для предоставления доступа к документу
    /// </summary>
    public class GrantAccessViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите пользователя")]
        [Display(Name = "Пользователь")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Уровень доступа")]
        public AccessLevel AccessLevel { get; set; } = AccessLevel.View;

        [Display(Name = "Роль")]
        public string? DocumentRole { get; set; }

        [Display(Name = "Разрешить скачивание")]
        public bool CanDownload { get; set; } = false;

        [Display(Name = "Разрешить печать")]
        public bool CanPrint { get; set; } = false;

        [Display(Name = "Разрешить экспорт")]
        public bool CanExport { get; set; } = false;

        [Display(Name = "Срок доступа")]
        [DataType(DataType.Date)]
        public DateTime? ExpiresAt { get; set; }

        [Display(Name = "Комментарий")]
        [StringLength(500)]
        public string? Comment { get; set; }

        public List<UserSelectViewModel> AvailableUsers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для блокировки документа
    /// </summary>
    public class BlockDocumentViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите причину блокировки")]
        [Display(Name = "Причина блокировки")]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Тип блокировки")]
        public BlockType BlockType { get; set; } = BlockType.ApprovalBlock;
    }

    /// <summary>
    /// ViewModel для снятия блокировки
    /// </summary>
    public class UnblockDocumentViewModel
    {
        public int DocumentId { get; set; }
        public int BlockId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string BlockReason { get; set; } = string.Empty;
        public BlockType BlockType { get; set; }

        [Display(Name = "Комментарий к снятию блокировки")]
        [StringLength(500)]
        public string? UnblockComment { get; set; }
    }

    /// <summary>
    /// ViewModel для архивации документа
    /// </summary>
    public class ArchiveDocumentViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;

        [Display(Name = "Срок хранения (месяцев)")]
        [Range(1, 1200, ErrorMessage = "Срок хранения от 1 до 1200 месяцев")]
        public int RetentionPeriodMonths { get; set; } = 60;

        [Display(Name = "Комментарий")]
        [StringLength(500)]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// ViewModel для делегирования согласования
    /// </summary>
    public class DelegateApprovalViewModel
    {
        public int ApprovalRequestId { get; set; }
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите пользователя")]
        [Display(Name = "Делегировать кому")]
        public string DelegateToUserId { get; set; } = string.Empty;

        [Display(Name = "Причина делегирования")]
        [StringLength(500)]
        public string? Reason { get; set; }

        public List<UserSelectViewModel> AvailableUsers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для запроса доработки
    /// </summary>
    public class RequestRevisionViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите причину доработки")]
        [Display(Name = "Причина доработки")]
        [StringLength(2000)]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Предложенные правки")]
        [StringLength(5000)]
        public string? SuggestedChanges { get; set; }

        [Display(Name = "Срок доработки")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// ViewModel для рецензирования документа
    /// </summary>
    public class ReviewDocumentViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public int? ApprovalRequestId { get; set; }

        [Required(ErrorMessage = "Введите комментарий рецензии")]
        [Display(Name = "Комментарий рецензии")]
        [StringLength(5000)]
        public string ReviewComment { get; set; } = string.Empty;

        [Display(Name = "Предложенные правки")]
        [StringLength(5000)]
        public string? SuggestedChanges { get; set; }

        [Display(Name = "Рекомендация")]
        public ReviewRecommendation Recommendation { get; set; } = ReviewRecommendation.NeedsRevision;
    }

    /// <summary>
    /// Рекомендация рецензента
    /// </summary>
    public enum ReviewRecommendation
    {
        [Display(Name = "Одобрить")]
        Approve = 0,

        [Display(Name = "Требуется доработка")]
        NeedsRevision = 1,

        [Display(Name = "Отклонить")]
        Reject = 2
    }

    /// <summary>
    /// ViewModel для экспорта документов (архивариус)
    /// </summary>
    public class ExportDocumentsViewModel
    {
        [Display(Name = "Документы для экспорта")]
        public List<int> DocumentIds { get; set; } = new();

        [Display(Name = "Формат экспорта")]
        public ExportFormat Format { get; set; } = ExportFormat.Zip;

        [Display(Name = "Включить метаданные")]
        public bool IncludeMetadata { get; set; } = true;

        [Display(Name = "Включить историю версий")]
        public bool IncludeVersionHistory { get; set; } = false;

        [Display(Name = "Включить комментарии")]
        public bool IncludeComments { get; set; } = false;
    }

    /// <summary>
    /// Формат экспорта
    /// </summary>
    public enum ExportFormat
    {
        [Display(Name = "ZIP-архив")]
        Zip = 0,

        [Display(Name = "PDF")]
        Pdf = 1,

        [Display(Name = "Excel (метаданные)")]
        Excel = 2
    }
}
