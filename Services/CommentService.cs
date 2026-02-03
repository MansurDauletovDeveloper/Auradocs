using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DocumentFlow.Services
{
    public interface ICommentService
    {
        Task<DocumentComment> AddCommentAsync(int documentId, string authorId, string text, int? parentCommentId = null);
        Task<bool> UpdateCommentAsync(int commentId, string userId, string newText);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        Task<List<DocumentCommentViewModel>> GetDocumentCommentsAsync(int documentId, string currentUserId);
    }

    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public CommentService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<DocumentComment> AddCommentAsync(int documentId, string authorId, string text, int? parentCommentId = null)
        {
            var comment = new DocumentComment
            {
                DocumentId = documentId,
                AuthorId = authorId,
                Text = text,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.DocumentComments.Add(comment);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(authorId, AuditActionType.AddComment,
                $"Добавлен комментарий к документу", documentId);

            return comment;
        }

        public async Task<bool> UpdateCommentAsync(int commentId, string userId, string newText)
        {
            var comment = await _context.DocumentComments.FindAsync(commentId);
            if (comment == null || comment.AuthorId != userId || comment.IsDeleted)
                return false;

            comment.Text = newText;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _context.DocumentComments.FindAsync(commentId);
            if (comment == null || comment.AuthorId != userId)
                return false;

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DocumentCommentViewModel>> GetDocumentCommentsAsync(int documentId, string currentUserId)
        {
            var comments = await _context.DocumentComments
                .Include(c => c.Author)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Author)
                .Where(c => c.DocumentId == documentId && !c.IsDeleted && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => MapComment(c, currentUserId)).ToList();
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
    }
}
