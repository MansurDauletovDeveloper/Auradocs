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
        public string RequestedByName { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status == ApprovalStatus.Pending;
    }

    public class ApprovalActionViewModel
    {
        public int RequestId { get; set; }
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите действие")]
        public bool Approve { get; set; }

        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }
    }

    public class PendingApprovalsViewModel
    {
        public List<ApprovalRequestViewModel> PendingApprovals { get; set; } = new();
        public int TotalCount { get; set; }
        public int OverdueCount { get; set; }
    }
}
