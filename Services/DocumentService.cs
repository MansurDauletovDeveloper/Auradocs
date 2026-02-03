using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DocumentFlow.Services
{
    public interface IDocumentService
    {
        Task<Document> CreateDocumentAsync(DocumentCreateViewModel model, string authorId);
        Task<Document?> GetDocumentAsync(int id);
        Task<DocumentDetailsViewModel?> GetDocumentDetailsAsync(int id, string currentUserId);
        Task<bool> UpdateDocumentAsync(DocumentEditViewModel model, string userId);
        Task<bool> DeleteDocumentAsync(int id, string userId);
        Task<(List<DocumentListViewModel> Documents, int TotalCount)> SearchDocumentsAsync(DocumentSearchViewModel search, string userId, bool isAdmin);
        Task<string> GenerateRegistrationNumberAsync();
        Task<bool> CanUserAccessDocumentAsync(int documentId, string userId, bool isAdmin);
        Task<bool> CanUserEditDocumentAsync(int documentId, string userId);
        Task<List<Document>> GetUserDocumentsAsync(string userId);
        Task<DocumentStatistics> GetStatisticsAsync(string? userId = null);
    }

    public class DocumentStatistics
    {
        public int TotalDocuments { get; set; }
        public int DraftsCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TodayCount { get; set; }
    }

    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IFileService _fileService;

        public DocumentService(ApplicationDbContext context, IAuditService auditService, IFileService fileService)
        {
            _context = context;
            _auditService = auditService;
            _fileService = fileService;
        }

        public async Task<Document> CreateDocumentAsync(DocumentCreateViewModel model, string authorId)
        {
            var document = new Document
            {
                Title = model.Title,
                Description = model.Description,
                DocumentType = model.DocumentType,
                TemplateId = model.TemplateId,
                AuthorId = authorId,
                Status = DocumentStatus.Draft,
                RegistrationNumber = await GenerateRegistrationNumberAsync(),
                CurrentVersion = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Создаём первую версию
            if (model.File != null)
            {
                var (filePath, fileName) = await _fileService.SaveFileAsync(model.File, document.Id);
                var version = new DocumentVersion
                {
                    DocumentId = document.Id,
                    VersionNumber = 1,
                    FileName = fileName,
                    FilePath = filePath,
                    ContentType = model.File.ContentType,
                    FileSize = model.File.Length,
                    FileHash = await _fileService.CalculateHashAsync(model.File),
                    CreatedById = authorId,
                    IsCurrent = true
                };
                _context.DocumentVersions.Add(version);
            }
            else if (!string.IsNullOrEmpty(model.Content))
            {
                var version = new DocumentVersion
                {
                    DocumentId = document.Id,
                    VersionNumber = 1,
                    FileName = $"{document.Title}.txt",
                    FilePath = string.Empty,
                    Content = model.Content,
                    CreatedById = authorId,
                    IsCurrent = true
                };
                _context.DocumentVersions.Add(version);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(authorId, AuditActionType.Create,
                $"Создан документ: {document.Title}", document.Id);

            return document;
        }

        public async Task<Document?> GetDocumentAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Author)
                .Include(d => d.Template)
                .Include(d => d.Versions)
                .Include(d => d.ApprovalRequests)
                    .ThenInclude(a => a.Approver)
                .Include(d => d.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<DocumentDetailsViewModel?> GetDocumentDetailsAsync(int id, string currentUserId)
        {
            var document = await GetDocumentAsync(id);
            if (document == null) return null;

            var isAuthor = document.AuthorId == currentUserId;

            return new DocumentDetailsViewModel
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                DocumentType = document.DocumentType,
                Status = document.Status,
                RegistrationNumber = document.RegistrationNumber,
                CurrentVersion = document.CurrentVersion,
                AuthorId = document.AuthorId,
                AuthorName = document.Author?.FullName ?? "Неизвестно",
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                TemplateName = document.Template?.Name,
                Versions = document.Versions
                    .OrderByDescending(v => v.VersionNumber)
                    .Select(v => new DocumentVersionViewModel
                    {
                        Id = v.Id,
                        VersionNumber = v.VersionNumber,
                        FileName = v.FileName,
                        FileSize = v.FileSize,
                        ChangeDescription = v.ChangeDescription,
                        CreatedByName = v.CreatedBy?.FullName ?? "Неизвестно",
                        CreatedAt = v.CreatedAt,
                        IsCurrent = v.IsCurrent
                    }).ToList(),
                ApprovalRequests = document.ApprovalRequests
                    .OrderBy(a => a.Order)
                    .Select(a => new ApprovalRequestViewModel
                    {
                        Id = a.Id,
                        DocumentId = a.DocumentId,
                        ApproverName = a.Approver?.FullName ?? "Неизвестно",
                        RequestedByName = a.RequestedBy?.FullName ?? "Неизвестно",
                        Status = a.Status,
                        Comment = a.Comment,
                        CreatedAt = a.CreatedAt,
                        ProcessedAt = a.ProcessedAt,
                        DueDate = a.DueDate
                    }).ToList(),
                Comments = document.Comments
                    .Where(c => !c.IsDeleted && c.ParentCommentId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => MapComment(c, currentUserId))
                    .ToList(),
                CanEdit = isAuthor && document.Status == DocumentStatus.Draft,
                CanDelete = isAuthor && document.Status == DocumentStatus.Draft,
                CanSubmitForApproval = isAuthor && (document.Status == DocumentStatus.Draft || document.Status == DocumentStatus.Rejected),
                CanApprove = document.ApprovalRequests.Any(a => a.ApproverId == currentUserId && a.Status == ApprovalStatus.Pending)
            };
        }

        private DocumentCommentViewModel MapComment(DocumentComment comment, string currentUserId)
        {
            return new DocumentCommentViewModel
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author?.FullName ?? "Неизвестно",
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ParentCommentId = comment.ParentCommentId,
                CanEdit = comment.AuthorId == currentUserId,
                CanDelete = comment.AuthorId == currentUserId,
                Replies = comment.Replies
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => MapComment(r, currentUserId))
                    .ToList()
            };
        }

        public async Task<bool> UpdateDocumentAsync(DocumentEditViewModel model, string userId)
        {
            var document = await _context.Documents.FindAsync(model.Id);
            if (document == null || document.AuthorId != userId || document.Status != DocumentStatus.Draft)
                return false;

            var oldTitle = document.Title;
            document.Title = model.Title;
            document.Description = model.Description;
            document.DocumentType = model.DocumentType;
            document.UpdatedAt = DateTime.UtcNow;

            // Если загружен новый файл - создаём новую версию
            if (model.NewFile != null)
            {
                // Помечаем предыдущие версии как не текущие
                var currentVersions = await _context.DocumentVersions
                    .Where(v => v.DocumentId == document.Id && v.IsCurrent)
                    .ToListAsync();
                foreach (var v in currentVersions)
                    v.IsCurrent = false;

                document.CurrentVersion++;

                var (filePath, fileName) = await _fileService.SaveFileAsync(model.NewFile, document.Id);
                var version = new DocumentVersion
                {
                    DocumentId = document.Id,
                    VersionNumber = document.CurrentVersion,
                    FileName = fileName,
                    FilePath = filePath,
                    ContentType = model.NewFile.ContentType,
                    FileSize = model.NewFile.Length,
                    FileHash = await _fileService.CalculateHashAsync(model.NewFile),
                    ChangeDescription = model.ChangeDescription,
                    CreatedById = userId,
                    IsCurrent = true
                };
                _context.DocumentVersions.Add(version);

                await _auditService.LogAsync(userId, AuditActionType.UploadVersion,
                    $"Загружена версия {document.CurrentVersion} документа: {document.Title}", document.Id);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, AuditActionType.Edit,
                $"Редактирование документа: {document.Title}", document.Id,
                oldValue: oldTitle != model.Title ? oldTitle : null,
                newValue: oldTitle != model.Title ? model.Title : null);

            return true;
        }

        public async Task<bool> DeleteDocumentAsync(int id, string userId)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null || (document.AuthorId != userId && document.Status != DocumentStatus.Draft))
                return false;

            var title = document.Title;

            // Удаляем файлы версий
            var versions = await _context.DocumentVersions
                .Where(v => v.DocumentId == id)
                .ToListAsync();

            foreach (var version in versions)
            {
                if (!string.IsNullOrEmpty(version.FilePath))
                    _fileService.DeleteFile(version.FilePath);
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, AuditActionType.Delete,
                $"Удалён документ: {title}", id);

            return true;
        }

        public async Task<(List<DocumentListViewModel> Documents, int TotalCount)> SearchDocumentsAsync(
            DocumentSearchViewModel search, string userId, bool isAdmin)
        {
            var query = _context.Documents
                .Include(d => d.Author)
                .AsQueryable();

            // Если не админ - показываем только свои документы или документы на согласование
            if (!isAdmin)
            {
                var approvalDocumentIds = await _context.ApprovalRequests
                    .Where(a => a.ApproverId == userId)
                    .Select(a => a.DocumentId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(d => d.AuthorId == userId || approvalDocumentIds.Contains(d.Id));
            }

            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                var term = search.SearchTerm.ToLower();
                query = query.Where(d => 
                    d.Title.ToLower().Contains(term) || 
                    d.RegistrationNumber.ToLower().Contains(term) ||
                    (d.Description != null && d.Description.ToLower().Contains(term)));
            }

            if (search.DocumentType.HasValue)
                query = query.Where(d => d.DocumentType == search.DocumentType.Value);

            if (search.Status.HasValue)
                query = query.Where(d => d.Status == search.Status.Value);

            if (!string.IsNullOrEmpty(search.AuthorId))
                query = query.Where(d => d.AuthorId == search.AuthorId);

            if (search.DateFrom.HasValue)
                query = query.Where(d => d.CreatedAt >= search.DateFrom.Value);

            if (search.DateTo.HasValue)
                query = query.Where(d => d.CreatedAt <= search.DateTo.Value.AddDays(1));

            var totalCount = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.UpdatedAt)
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(d => new DocumentListViewModel
                {
                    Id = d.Id,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    Status = d.Status,
                    RegistrationNumber = d.RegistrationNumber,
                    AuthorName = d.Author != null ? d.Author.LastName + " " + d.Author.FirstName : "Неизвестно",
                    CreatedAt = d.CreatedAt,
                    CurrentVersion = d.CurrentVersion
                })
                .ToListAsync();

            return (documents, totalCount);
        }

        public async Task<string> GenerateRegistrationNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.Documents
                .CountAsync(d => d.CreatedAt.Year == year);
            
            return $"ДОК-{year}-{(count + 1):D5}";
        }

        public async Task<bool> CanUserAccessDocumentAsync(int documentId, string userId, bool isAdmin)
        {
            if (isAdmin) return true;

            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            if (document.AuthorId == userId) return true;

            // Проверяем, есть ли запрос на согласование для этого пользователя
            return await _context.ApprovalRequests
                .AnyAsync(a => a.DocumentId == documentId && a.ApproverId == userId);
        }

        public async Task<bool> CanUserEditDocumentAsync(int documentId, string userId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            return document != null && 
                   document.AuthorId == userId && 
                   (document.Status == DocumentStatus.Draft || document.Status == DocumentStatus.Rejected);
        }

        public async Task<List<Document>> GetUserDocumentsAsync(string userId)
        {
            return await _context.Documents
                .Where(d => d.AuthorId == userId)
                .Include(d => d.Author)
                .OrderByDescending(d => d.UpdatedAt)
                .ToListAsync();
        }

        public async Task<DocumentStatistics> GetStatisticsAsync(string? userId = null)
        {
            var query = _context.Documents.AsQueryable();
            
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(d => d.AuthorId == userId);

            var today = DateTime.UtcNow.Date;

            return new DocumentStatistics
            {
                TotalDocuments = await query.CountAsync(),
                DraftsCount = await query.CountAsync(d => d.Status == DocumentStatus.Draft),
                PendingCount = await query.CountAsync(d => d.Status == DocumentStatus.Pending),
                ApprovedCount = await query.CountAsync(d => d.Status == DocumentStatus.Approved),
                RejectedCount = await query.CountAsync(d => d.Status == DocumentStatus.Rejected),
                TodayCount = await query.CountAsync(d => d.CreatedAt >= today)
            };
        }
    }
}
