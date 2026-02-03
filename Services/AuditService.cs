using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentFlow.Services
{
    public interface IAuditService
    {
        Task LogAsync(string userId, AuditActionType actionType, string description, 
            int? documentId = null, string? entityName = null, string? entityId = null,
            string? oldValue = null, string? newValue = null);
        Task LogAsync(AuditLog log);
        Task<(List<AuditLog> Logs, int TotalCount)> GetLogsAsync(
            string? userId = null, AuditActionType? actionType = null, int? documentId = null,
            DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 25);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string userId, AuditActionType actionType, string description,
            int? documentId = null, string? entityName = null, string? entityId = null,
            string? oldValue = null, string? newValue = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var log = new AuditLog
            {
                UserId = userId,
                ActionType = actionType,
                ActionDescription = description,
                DocumentId = documentId,
                EntityName = entityName,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow
            };

            await LogAsync(log);
        }

        public async Task LogAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> GetLogsAsync(
            string? userId = null, AuditActionType? actionType = null, int? documentId = null,
            DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 25)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.Document)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);

            if (actionType.HasValue)
                query = query.Where(a => a.ActionType == actionType.Value);

            if (documentId.HasValue)
                query = query.Where(a => a.DocumentId == documentId.Value);

            if (dateFrom.HasValue)
                query = query.Where(a => a.Timestamp >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(a => a.Timestamp <= dateTo.Value.AddDays(1));

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
    }
}
