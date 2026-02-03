using DocumentFlow.Models.Entities;

namespace DocumentFlow.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // Статистика для сотрудника
        public int MyDocumentsCount { get; set; }
        public int MyDraftsCount { get; set; }
        public int MyPendingCount { get; set; }
        public int MyApprovedCount { get; set; }
        public int MyRejectedCount { get; set; }

        // Статистика для руководителя
        public int PendingApprovalsCount { get; set; }
        public int OverdueApprovalsCount { get; set; }

        // Статистика для администратора
        public int TotalUsersCount { get; set; }
        public int TotalDocumentsCount { get; set; }
        public int TodayDocumentsCount { get; set; }
        public int TodayApprovalsCount { get; set; }

        // Последние документы
        public List<DocumentListViewModel> RecentDocuments { get; set; } = new();

        // Ожидающие согласования (для руководителя)
        public List<ApprovalRequestViewModel> PendingApprovals { get; set; } = new();

        // Непрочитанные уведомления
        public int UnreadNotificationsCount { get; set; }
        public List<NotificationViewModel> RecentNotifications { get; set; } = new();
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public int? DocumentId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
