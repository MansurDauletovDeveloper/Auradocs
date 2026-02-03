using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentFlow.Services
{
    public interface IDocumentVersionService
    {
        Task<DocumentVersion?> GetVersionAsync(int versionId);
        Task<List<DocumentVersion>> GetDocumentVersionsAsync(int documentId);
        Task<DocumentVersion?> GetCurrentVersionAsync(int documentId);
        Task<bool> RestoreVersionAsync(int versionId, string userId);
        Task<byte[]?> DownloadVersionAsync(int versionId, string userId);
    }

    public class DocumentVersionService : IDocumentVersionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;

        public DocumentVersionService(ApplicationDbContext context, IFileService fileService, IAuditService auditService)
        {
            _context = context;
            _fileService = fileService;
            _auditService = auditService;
        }

        public async Task<DocumentVersion?> GetVersionAsync(int versionId)
        {
            return await _context.DocumentVersions
                .Include(v => v.Document)
                .Include(v => v.CreatedBy)
                .FirstOrDefaultAsync(v => v.Id == versionId);
        }

        public async Task<List<DocumentVersion>> GetDocumentVersionsAsync(int documentId)
        {
            return await _context.DocumentVersions
                .Include(v => v.CreatedBy)
                .Where(v => v.DocumentId == documentId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();
        }

        public async Task<DocumentVersion?> GetCurrentVersionAsync(int documentId)
        {
            return await _context.DocumentVersions
                .Include(v => v.CreatedBy)
                .FirstOrDefaultAsync(v => v.DocumentId == documentId && v.IsCurrent);
        }

        public async Task<bool> RestoreVersionAsync(int versionId, string userId)
        {
            var version = await _context.DocumentVersions
                .Include(v => v.Document)
                .FirstOrDefaultAsync(v => v.Id == versionId);

            if (version == null || version.Document == null)
                return false;

            // Проверяем права на редактирование
            if (version.Document.AuthorId != userId || 
                (version.Document.Status != DocumentStatus.Draft && version.Document.Status != DocumentStatus.Rejected))
                return false;

            // Помечаем все версии как не текущие
            var allVersions = await _context.DocumentVersions
                .Where(v => v.DocumentId == version.DocumentId)
                .ToListAsync();

            foreach (var v in allVersions)
                v.IsCurrent = false;

            // Создаём новую версию на основе восстанавливаемой
            var newVersion = new DocumentVersion
            {
                DocumentId = version.DocumentId,
                VersionNumber = version.Document.CurrentVersion + 1,
                FileName = version.FileName,
                FilePath = version.FilePath,
                ContentType = version.ContentType,
                FileSize = version.FileSize,
                FileHash = version.FileHash,
                Content = version.Content,
                ChangeDescription = $"Восстановлено из версии {version.VersionNumber}",
                CreatedById = userId,
                IsCurrent = true
            };

            _context.DocumentVersions.Add(newVersion);

            version.Document.CurrentVersion++;
            version.Document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, AuditActionType.RestoreVersion,
                $"Восстановлена версия {version.VersionNumber} документа \"{version.Document.Title}\"",
                version.DocumentId);

            return true;
        }

        public async Task<byte[]?> DownloadVersionAsync(int versionId, string userId)
        {
            var version = await GetVersionAsync(versionId);
            if (version == null)
                return null;

            await _auditService.LogAsync(userId, AuditActionType.Download,
                $"Скачана версия {version.VersionNumber} документа", version.DocumentId);

            if (!string.IsNullOrEmpty(version.FilePath))
            {
                return await _fileService.GetFileAsync(version.FilePath);
            }
            else if (!string.IsNullOrEmpty(version.Content))
            {
                return System.Text.Encoding.UTF8.GetBytes(version.Content);
            }

            return null;
        }
    }
}
