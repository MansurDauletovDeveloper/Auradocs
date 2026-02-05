using DocumentFlow.Models.Entities;

namespace DocumentFlow.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string PrimaryRole { get; set; } = string.Empty;

        // Статистика для сотрудника
        public int MyDocumentsCount { get; set; }
        public int MyDraftsCount { get; set; }
        public int MyPendingCount { get; set; }
        public int MyApprovedCount { get; set; }
        public int MyRejectedCount { get; set; }
        public int MyOnRevisionCount { get; set; }

        // Статистика для владельца документа
        public int OwnedDocumentsCount { get; set; }
        public int OwnedPendingCount { get; set; }

        // Статистика для руководителя
        public int PendingApprovalsCount { get; set; }
        public int OverdueApprovalsCount { get; set; }
        public int DelegatedApprovalsCount { get; set; }
        public int SubordinatesCount { get; set; }
        public int DepartmentDocumentsCount { get; set; }

        // Статистика для юридического отдела
        public int LegalReviewPendingCount { get; set; }
        public int BlockedDocumentsCount { get; set; }

        // Статистика для рецензента
        public int PendingReviewsCount { get; set; }
        public int CompletedReviewsThisMonthCount { get; set; }

        // Статистика для аудитора
        public int TodayAuditLogsCount { get; set; }
        public int WeekAuditLogsCount { get; set; }

        // Статистика для архивариуса
        public int ArchivedDocumentsCount { get; set; }
        public int PendingDeletionCount { get; set; }
        public int ExpiringRetentionCount { get; set; }

        // Статистика для администратора
        public int TotalUsersCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public int ExternalUsersCount { get; set; }
        public int TotalDocumentsCount { get; set; }
        public int TodayDocumentsCount { get; set; }
        public int TodayApprovalsCount { get; set; }
        public int ActiveDelegationsCount { get; set; }

        // Статистика для должностного лица по соблюдению требований
        public int ConfidentialDocumentsCount { get; set; }
        public int AccessViolationsCount { get; set; }
        public int PendingAccessReviewsCount { get; set; }

        // Статистика для ИТ / Интегратор
        public int ActiveIntegrationsCount { get; set; }
        public int FailedTasksCount { get; set; }
        public int QueuedTasksCount { get; set; }

        // Последние документы
        public List<DocumentListViewModel> RecentDocuments { get; set; } = new();

        // Ожидающие согласования (для руководителя)
        public List<ApprovalRequestViewModel> PendingApprovals { get; set; } = new();

        // Непрочитанные уведомления
        public int UnreadNotificationsCount { get; set; }
        public List<NotificationViewModel> RecentNotifications { get; set; } = new();

        // Активные делегирования (для заместителя)
        public List<DelegationListViewModel> ActiveDelegationsToMe { get; set; } = new();

        // Документы с ограниченным доступом (для внешних пользователей)
        public List<DocumentListViewModel> AccessibleDocuments { get; set; } = new();
        public DateTime? AccessExpiresAt { get; set; }

        // Быстрые действия в зависимости от роли
        public bool CanCreateDocuments { get; set; }
        public bool CanApproveDocuments { get; set; }
        public bool CanReviewDocuments { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanViewAuditLogs { get; set; }
        public bool CanManageArchive { get; set; }
        public bool CanBlockDocuments { get; set; }
        public bool CanManageIntegrations { get; set; }
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

    /// <summary>
    /// ViewModel для панели аудитора
    /// </summary>
    public class AuditorDashboardViewModel
    {
        public int TotalLogsCount { get; set; }
        public int TodayLogsCount { get; set; }
        public int WeekLogsCount { get; set; }
        public int MonthLogsCount { get; set; }

        public Dictionary<string, int> ActionTypeStats { get; set; } = new();
        public Dictionary<string, int> UserActivityStats { get; set; } = new();
        public List<AuditLogViewModel> RecentLogs { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для панели архивариуса
    /// </summary>
    public class ArchivistDashboardViewModel
    {
        public int TotalArchivedCount { get; set; }
        public int PendingArchiveCount { get; set; }
        public int ExpiringRetentionCount { get; set; }
        public int ScheduledDeletionCount { get; set; }

        public long TotalArchiveSize { get; set; }
        public Dictionary<DocumentType, int> DocumentTypeStats { get; set; } = new();
        public List<DocumentListViewModel> RecentlyArchived { get; set; } = new();
        public List<DocumentListViewModel> ExpiringDocuments { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для панели юридического отдела
    /// </summary>
    public class LegalDashboardViewModel
    {
        public int PendingLegalReviewCount { get; set; }
        public int ActiveBlocksCount { get; set; }
        public int CompletedReviewsThisMonthCount { get; set; }
        public int LegalDocumentsCount { get; set; }

        public List<ApprovalRequestViewModel> PendingLegalApprovals { get; set; } = new();
        public List<DocumentBlockViewModel> ActiveBlocks { get; set; } = new();
        public List<DocumentListViewModel> RecentLegalDocuments { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для панели должностного лица по соблюдению требований
    /// </summary>
    public class ComplianceDashboardViewModel
    {
        public int ConfidentialDocumentsCount { get; set; }
        public int SecretDocumentsCount { get; set; }
        public int ExternalAccessCount { get; set; }
        public int ExpiredAccessCount { get; set; }

        public Dictionary<ConfidentialityLevel, int> ConfidentialityStats { get; set; } = new();
        public List<DocumentAccessViewModel> RecentAccessGrants { get; set; } = new();
        public List<UserListViewModel> ExternalUsers { get; set; } = new();
    }
}
