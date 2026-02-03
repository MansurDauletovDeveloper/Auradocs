using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocumentFlow.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public NotificationsController(
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id, 50);
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _notificationService.MarkAsReadAsync(id, user.Id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _notificationService.MarkAllAsReadAsync(user.Id);
            TempData["SuccessMessage"] = "Все уведомления отмечены как прочитанные.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var count = await _notificationService.GetUnreadCountAsync(user.Id);
            return Json(new { count });
        }
    }
}
