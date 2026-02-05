using System.ComponentModel.DataAnnotations;
using DocumentFlow.Models.Entities;

namespace DocumentFlow.Models.ViewModels
{
    public class ApprovalRequestViewModel
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentRegistrationNumber { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; }
        public string ApproverName { get; set; } = string.Empty;
        public string ApproverId { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status == ApprovalStatus.Pending;
        
        // Новые поля
        public ApproverType ApproverType { get; set; }
        public bool IsRequired { get; set; }
        public bool CanBlock { get; set; }
        public string? DelegatedToName { get; set; }
        public DateTime? DelegatedAt { get; set; }
        public string? DelegationReason { get; set; }
        public string? ReviewComment { get; set; }
        public string? SuggestedChanges { get; set; }
        public ConfidentialityLevel DocumentConfidentiality { get; set; }
        public bool DocumentRequiresLegalReview { get; set; }
    }

    public class ApprovalActionViewModel
    {
        public int RequestId { get; set; }
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public ApproverType ApproverType { get; set; }

        [Required(ErrorMessage = "Выберите действие")]
        public ApprovalAction Action { get; set; }

        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }

        [Display(Name = "Предложенные правки")]
        public string? SuggestedChanges { get; set; }

        [Display(Name = "Блокировать утверждение")]
        public bool BlockApproval { get; set; } = false;

        [Display(Name = "Причина блокировки")]
        public string? BlockReason { get; set; }
    }

    /// <summary>
    /// Действие при согласовании
    /// </summary>
    public enum ApprovalAction
    {
        [Display(Name = "Утвердить")]
        Approve = 0,

        [Display(Name = "Отклонить")]
        Reject = 1,

        [Display(Name = "На доработку")]
        RequestRevision = 2,

        [Display(Name = "Делегировать")]
        Delegate = 3,

        [Display(Name = "На юридическую проверку")]
        SendToLegalReview = 4,

        [Display(Name = "Заблокировать")]
        Block = 5
    }

    public class PendingApprovalsViewModel
    {
        public List<ApprovalRequestViewModel> PendingApprovals { get; set; } = new();
        public int TotalCount { get; set; }
        public int OverdueCount { get; set; }
        public int LegalReviewCount { get; set; }
        public int DelegatedCount { get; set; }
        public int RevisionCount { get; set; }

        // Фильтры
        public ApproverType? FilterByType { get; set; }
        public ApprovalStatus? FilterByStatus { get; set; }
        public bool ShowOverdueOnly { get; set; }
    }

    /// <summary>
    /// ViewModel для статистики руководителя по подразделению
    /// </summary>
    public class DepartmentDocumentsViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public List<DocumentListViewModel> Documents { get; set; } = new();
        public int TotalDocuments { get; set; }
        public int PendingApproval { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int OnRevision { get; set; }
        public int OnLegalReview { get; set; }
    }

    /// <summary>
    /// ViewModel для статистики согласований руководителя
    /// </summary>
    public class ManagerApprovalStatsViewModel
    {
        public int TotalPending { get; set; }
        public int ApprovedThisMonth { get; set; }
        public int RejectedThisMonth { get; set; }
        public int DelegatedThisMonth { get; set; }
        public int OverdueCount { get; set; }
        public double AverageApprovalTimeHours { get; set; }
        public List<SubordinateStatsViewModel> SubordinateStats { get; set; } = new();
    }

    /// <summary>
    /// Статистика по подчинённому
    /// </summary>
    public class SubordinateStatsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int DocumentsCreated { get; set; }
        public int DocumentsPending { get; set; }
        public int DocumentsApproved { get; set; }
        public int DocumentsRejected { get; set; }
    }
}
