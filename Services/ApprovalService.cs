using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DocumentFlow.Services
{
    public interface IApprovalService
    {
        Task<bool> SubmitForApprovalAsync(int documentId, List<string> approverIds, string requesterId, string? comment, DateTime? dueDate);
        Task<bool> ApproveAsync(int requestId, string approverId, string? comment);
        Task<bool> RejectAsync(int requestId, string approverId, string comment);
        Task<List<ApprovalRequestViewModel>> GetPendingApprovalsAsync(string approverId);
        Task<ApprovalRequest?> GetApprovalRequestAsync(int requestId);
        Task<int> GetPendingApprovalsCountAsync(string approverId);
        Task<int> GetOverdueApprovalsCountAsync(string approverId);
    }

    public class ApprovalService : IApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public ApprovalService(ApplicationDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<bool> SubmitForApprovalAsync(int documentId, List<string> approverIds, string requesterId, string? comment, DateTime? dueDate)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null || 
                document.AuthorId != requesterId || 
                (document.Status != DocumentStatus.Draft && document.Status != DocumentStatus.Rejected))
                return false;

            // Отменяем предыдущие запросы на согласование
            var oldRequests = await _context.ApprovalRequests
                .Where(a => a.DocumentId == documentId && a.Status == ApprovalStatus.Pending)
                .ToListAsync();
            
            foreach (var req in oldRequests)
            {
                req.Status = ApprovalStatus.Cancelled;
            }

            // Создаём новые запросы
            int order = 1;
            foreach (var approverId in approverIds)
            {
                var request = new ApprovalRequest
                {
                    DocumentId = documentId,
                    ApproverId = approverId,
                    RequestedById = requesterId,
                    Status = ApprovalStatus.Pending,
                    Order = order++,
                    DueDate = dueDate,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ApprovalRequests.Add(request);

                // Отправляем уведомление
                await _notificationService.CreateNotificationAsync(
                    approverId,
                    "Документ на согласование",
                    $"Документ \"{document.Title}\" ожидает вашего согласования",
                    NotificationType.ApprovalRequest,
                    documentId);
            }

            document.Status = DocumentStatus.Pending;
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(requesterId, AuditActionType.SubmitForApproval,
                $"Документ \"{document.Title}\" отправлен на согласование", documentId);

            return true;
        }

        public async Task<bool> ApproveAsync(int requestId, string approverId, string? comment)
        {
            var request = await _context.ApprovalRequests
                .Include(a => a.Document)
                .FirstOrDefaultAsync(a => a.Id == requestId);

            if (request == null || request.ApproverId != approverId || request.Status != ApprovalStatus.Pending)
                return false;

            request.Status = ApprovalStatus.Approved;
            request.Comment = comment;
            request.ProcessedAt = DateTime.UtcNow;

            // Проверяем, все ли согласующие утвердили
            var allRequests = await _context.ApprovalRequests
                .Where(a => a.DocumentId == request.DocumentId && a.Status != ApprovalStatus.Cancelled)
                .ToListAsync();

            var allApproved = allRequests.All(a => a.Status == ApprovalStatus.Approved);

            if (allApproved && request.Document != null)
            {
                request.Document.Status = DocumentStatus.Approved;
                request.Document.UpdatedAt = DateTime.UtcNow;

                // Уведомляем автора
                await _notificationService.CreateNotificationAsync(
                    request.Document.AuthorId,
                    "Документ утверждён",
                    $"Документ \"{request.Document.Title}\" был утверждён",
                    NotificationType.DocumentApproved,
                    request.DocumentId);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(approverId, AuditActionType.Approve,
                $"Документ \"{request.Document?.Title}\" утверждён", request.DocumentId);

            return true;
        }

        public async Task<bool> RejectAsync(int requestId, string approverId, string comment)
        {
            var request = await _context.ApprovalRequests
                .Include(a => a.Document)
                .FirstOrDefaultAsync(a => a.Id == requestId);

            if (request == null || request.ApproverId != approverId || request.Status != ApprovalStatus.Pending)
                return false;

            if (string.IsNullOrWhiteSpace(comment))
                return false;

            request.Status = ApprovalStatus.Rejected;
            request.Comment = comment;
            request.ProcessedAt = DateTime.UtcNow;

            if (request.Document != null)
            {
                request.Document.Status = DocumentStatus.Rejected;
                request.Document.UpdatedAt = DateTime.UtcNow;

                // Уведомляем автора
                await _notificationService.CreateNotificationAsync(
                    request.Document.AuthorId,
                    "Документ отклонён",
                    $"Документ \"{request.Document.Title}\" был отклонён. Причина: {comment}",
                    NotificationType.DocumentRejected,
                    request.DocumentId);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(approverId, AuditActionType.Reject,
                $"Документ \"{request.Document?.Title}\" отклонён. Причина: {comment}", request.DocumentId);

            return true;
        }

        public async Task<List<ApprovalRequestViewModel>> GetPendingApprovalsAsync(string approverId)
        {
            return await _context.ApprovalRequests
                .Include(a => a.Document)
                .Include(a => a.RequestedBy)
                .Include(a => a.Approver)
                .Where(a => a.ApproverId == approverId && a.Status == ApprovalStatus.Pending)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.CreatedAt)
                .Select(a => new ApprovalRequestViewModel
                {
                    Id = a.Id,
                    DocumentId = a.DocumentId,
                    DocumentTitle = a.Document != null ? a.Document.Title : "Неизвестно",
                    DocumentRegistrationNumber = a.Document != null ? a.Document.RegistrationNumber : "",
                    DocumentType = a.Document != null ? a.Document.DocumentType : DocumentType.Other,
                    ApproverName = a.Approver != null ? a.Approver.LastName + " " + a.Approver.FirstName : "Неизвестно",
                    RequestedByName = a.RequestedBy != null ? a.RequestedBy.LastName + " " + a.RequestedBy.FirstName : "Неизвестно",
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    DueDate = a.DueDate
                })
                .ToListAsync();
        }

        public async Task<ApprovalRequest?> GetApprovalRequestAsync(int requestId)
        {
            return await _context.ApprovalRequests
                .Include(a => a.Document)
                .Include(a => a.Approver)
                .Include(a => a.RequestedBy)
                .FirstOrDefaultAsync(a => a.Id == requestId);
        }

        public async Task<int> GetPendingApprovalsCountAsync(string approverId)
        {
            return await _context.ApprovalRequests
                .CountAsync(a => a.ApproverId == approverId && a.Status == ApprovalStatus.Pending);
        }

        public async Task<int> GetOverdueApprovalsCountAsync(string approverId)
        {
            var now = DateTime.UtcNow;
            return await _context.ApprovalRequests
                .CountAsync(a => a.ApproverId == approverId && 
                                 a.Status == ApprovalStatus.Pending && 
                                 a.DueDate.HasValue && 
                                 a.DueDate.Value < now);
        }
    }
}
