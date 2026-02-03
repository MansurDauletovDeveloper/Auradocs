using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocumentFlow.Controllers
{
    [Authorize(Roles = "Manager,Administrator")]
    public class ApprovalController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApprovalService _approvalService;
        private readonly IDocumentService _documentService;

        public ApprovalController(
            UserManager<ApplicationUser> userManager,
            IApprovalService approvalService,
            IDocumentService documentService)
        {
            _userManager = userManager;
            _approvalService = approvalService;
            _documentService = documentService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var pendingApprovals = await _approvalService.GetPendingApprovalsAsync(user.Id);
            var overdueCount = await _approvalService.GetOverdueApprovalsCountAsync(user.Id);

            var model = new PendingApprovalsViewModel
            {
                PendingApprovals = pendingApprovals,
                TotalCount = pendingApprovals.Count,
                OverdueCount = overdueCount
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var request = await _approvalService.GetApprovalRequestAsync(id);
            if (request == null || request.ApproverId != user.Id || request.Status != ApprovalStatus.Pending)
                return NotFound();

            var document = await _documentService.GetDocumentDetailsAsync(request.DocumentId, user.Id);
            if (document == null) return NotFound();

            ViewBag.ApprovalRequest = new ApprovalRequestViewModel
            {
                Id = request.Id,
                DocumentId = request.DocumentId,
                DocumentTitle = request.Document?.Title ?? "",
                RequestedByName = request.RequestedBy?.FullName ?? "",
                CreatedAt = request.CreatedAt,
                DueDate = request.DueDate
            };

            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApprovalActionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _approvalService.ApproveAsync(model.RequestId, user.Id, model.Comment);
            
            if (!result)
            {
                TempData["ErrorMessage"] = "Не удалось утвердить документ.";
                return RedirectToAction(nameof(Review), new { id = model.RequestId });
            }

            TempData["SuccessMessage"] = "Документ утверждён.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(ApprovalActionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.Comment))
            {
                TempData["ErrorMessage"] = "Укажите причину отклонения.";
                return RedirectToAction(nameof(Review), new { id = model.RequestId });
            }

            var result = await _approvalService.RejectAsync(model.RequestId, user.Id, model.Comment);
            
            if (!result)
            {
                TempData["ErrorMessage"] = "Не удалось отклонить документ.";
                return RedirectToAction(nameof(Review), new { id = model.RequestId });
            }

            TempData["SuccessMessage"] = "Документ отклонён.";
            return RedirectToAction(nameof(Index));
        }
    }
}
