using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            var isAdmin = roles.Contains("Administrator");
            var isManager = roles.Contains("Manager");

            var model = new DashboardViewModel
            {
                UserName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Сотрудник"
            };

            // Статистика для пользователя
            var userStats = await _documentService.GetStatisticsAsync(user.Id);
            model.MyDocumentsCount = userStats.TotalDocuments;
            model.MyDraftsCount = userStats.DraftsCount;
            model.MyPendingCount = userStats.PendingCount;
            model.MyApprovedCount = userStats.ApprovedCount;
            model.MyRejectedCount = userStats.RejectedCount;

            // Статистика для руководителя
            if (isManager || isAdmin)
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
