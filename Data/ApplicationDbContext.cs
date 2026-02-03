using DocumentFlow.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocumentFlow.Data
{
    /// <summary>
    /// Контекст базы данных системы документооборота
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<DocumentComment> DocumentComments { get; set; }
        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Document configuration
            builder.Entity<Document>(entity =>
            {
                entity.HasIndex(d => d.RegistrationNumber).IsUnique();
                entity.HasIndex(d => d.Status);
                entity.HasIndex(d => d.CreatedAt);
                entity.HasIndex(d => d.DocumentType);

                entity.HasOne(d => d.Author)
                    .WithMany(u => u.CreatedDocuments)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Template)
                    .WithMany(t => t.Documents)
                    .HasForeignKey(d => d.TemplateId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // DocumentVersion configuration
            builder.Entity<DocumentVersion>(entity =>
            {
                entity.HasIndex(v => new { v.DocumentId, v.VersionNumber }).IsUnique();

                entity.HasOne(v => v.Document)
                    .WithMany(d => d.Versions)
                    .HasForeignKey(v => v.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.CreatedBy)
                    .WithMany()
                    .HasForeignKey(v => v.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApprovalRequest configuration
            builder.Entity<ApprovalRequest>(entity =>
            {
                entity.HasIndex(a => a.Status);
                entity.HasIndex(a => new { a.DocumentId, a.ApproverId });

                entity.HasOne(a => a.Document)
                    .WithMany(d => d.ApprovalRequests)
                    .HasForeignKey(a => a.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Approver)
                    .WithMany(u => u.ApprovalRequests)
                    .HasForeignKey(a => a.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.RequestedBy)
                    .WithMany()
                    .HasForeignKey(a => a.RequestedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.Timestamp);
                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => a.ActionType);
                entity.HasIndex(a => a.DocumentId);

                entity.HasOne(a => a.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Document)
                    .WithMany(d => d.AuditLogs)
                    .HasForeignKey(a => a.DocumentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Notification configuration
            builder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.IsRead);
                entity.HasIndex(n => n.CreatedAt);

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Document)
                    .WithMany()
                    .HasForeignKey(n => n.DocumentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // DocumentComment configuration
            builder.Entity<DocumentComment>(entity =>
            {
                entity.HasIndex(c => c.DocumentId);
                entity.HasIndex(c => c.CreatedAt);

                entity.HasOne(c => c.Document)
                    .WithMany(d => d.Comments)
                    .HasForeignKey(c => c.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Author)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DocumentTemplate configuration
            builder.Entity<DocumentTemplate>(entity =>
            {
                entity.HasIndex(t => t.Name);
                entity.HasIndex(t => t.DocumentType);
            });
        }
    }
}
