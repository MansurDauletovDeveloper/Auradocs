using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static DocumentFlow.Models.Entities.SystemRoles;

namespace DocumentFlow.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDocumentService _documentService;
        private readonly IApprovalService _approvalService;
        private readonly INotificationService _notificationService;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IDocumentService documentService,
            IApprovalService approvalService,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _documentService = documentService;
            _approvalService = approvalService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains(SystemRoles.Administrator);
            var isManager = roles.Contains(SystemRoles.Manager);
            var isLegal = roles.Contains(SystemRoles.LegalDepartment);
            var isAuditor = roles.Contains(SystemRoles.Auditor);
            var isArchivist = roles.Contains(SystemRoles.Archivist);
            var isReviewer = roles.Contains(SystemRoles.Reviewer);
            var isComplianceOfficer = roles.Contains(SystemRoles.ComplianceOfficer);
            var isDeputy = roles.Contains(SystemRoles.Deputy);
            var isExternalUser = roles.Contains(SystemRoles.ExternalUser);
            var isViewer = roles.Contains(SystemRoles.Viewer);

            var model = new DashboardViewModel
            {
                UserName = user.FullName,
                UserId = user.Id,
                Roles = roles.ToList(),
                PrimaryRole = roles.FirstOrDefault() ?? SystemRoles.Employee,
                
                // Права на основе ролей
                CanCreateDocuments = !isAuditor && !isViewer && !isExternalUser,
                CanApproveDocuments = isAdmin || isManager || isLegal || isDeputy,
                CanReviewDocuments = isReviewer || isLegal,
                CanManageUsers = isAdmin,
                CanViewAuditLogs = isAdmin || isAuditor || isComplianceOfficer,
                CanManageArchive = isAdmin || isArchivist,
                CanBlockDocuments = isAdmin || isLegal || isComplianceOfficer,
                CanManageIntegrations = isAdmin || roles.Contains(SystemRoles.ITIntegrator)
            };

            // Статистика для пользователя
            var userStats = await _documentService.GetStatisticsAsync(user.Id);
            model.MyDocumentsCount = userStats.TotalDocuments;
            model.MyDraftsCount = userStats.DraftsCount;
            model.MyPendingCount = userStats.PendingCount;
            model.MyApprovedCount = userStats.ApprovedCount;
            model.MyRejectedCount = userStats.RejectedCount;

            // Статистика для руководителя
            if (isManager || isAdmin || isDeputy)
            {
                model.PendingApprovalsCount = await _approvalService.GetPendingApprovalsCountAsync(user.Id);
                model.OverdueApprovalsCount = await _approvalService.GetOverdueApprovalsCountAsync(user.Id);
                model.PendingApprovals = await _approvalService.GetPendingApprovalsAsync(user.Id);
            }

            // Статистика для администратора
            if (isAdmin)
            {
                var totalStats = await _documentService.GetStatisticsAsync();
                model.TotalDocumentsCount = totalStats.TotalDocuments;
                model.TodayDocumentsCount = totalStats.TodayCount;
                model.TotalUsersCount = _userManager.Users.Count();
                model.ActiveUsersCount = _userManager.Users.Count(u => u.IsActive);
                model.ExternalUsersCount = _userManager.Users.Count(u => u.IsExternalUser);
            }

            // Последние документы пользователя
            var search = new DocumentSearchViewModel { PageSize = 5 };
            var (documents, _) = await _documentService.SearchDocumentsAsync(search, user.Id, isAdmin);
            model.RecentDocuments = documents;

            // Уведомления
            model.UnreadNotificationsCount = await _notificationService.GetUnreadCountAsync(user.Id);
            model.RecentNotifications = await _notificationService.GetUserNotificationsAsync(user.Id, 5);

            return View(model);
        }
    }
}
