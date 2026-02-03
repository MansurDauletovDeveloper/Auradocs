using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DocumentFlow.Services
{
    public interface ITemplateService
    {
        Task<List<DocumentTemplate>> GetAllTemplatesAsync();
        Task<List<DocumentTemplate>> GetActiveTemplatesAsync();
        Task<DocumentTemplate?> GetByIdAsync(int id);
        Task<DocumentTemplate> CreateAsync(TemplateCreateViewModel model, string userId);
        Task UpdateAsync(int id, TemplateEditViewModel model);
        Task DeleteAsync(int id);
        Task ToggleActiveAsync(int id);
    }

    public class TemplateService : ITemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TemplateService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<List<DocumentTemplate>> GetAllTemplatesAsync()
        {
            return await _context.DocumentTemplates
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<DocumentTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.DocumentTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<DocumentTemplate?> GetByIdAsync(int id)
        {
            return await _context.DocumentTemplates.FindAsync(id);
        }

        public async Task<DocumentTemplate> CreateAsync(TemplateCreateViewModel model, string userId)
        {
            var template = new DocumentTemplate
            {
                Name = model.Name,
                Description = model.Description,
                DocumentType = model.DocumentType,
                Content = model.Content,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            if (model.TemplateFile != null && model.TemplateFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "templates");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{Guid.NewGuid()}_{model.TemplateFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.TemplateFile.CopyToAsync(stream);
                }
                
                template.TemplatePath = $"/templates/{uniqueFileName}";
            }

            _context.DocumentTemplates.Add(template);
            await _context.SaveChangesAsync();
            
            return template;
        }

        public async Task UpdateAsync(int id, TemplateEditViewModel model)
        {
            var template = await _context.DocumentTemplates.FindAsync(id);
            if (template == null) return;

            template.Name = model.Name;
            template.Description = model.Description;
            template.DocumentType = model.DocumentType;
            template.Content = model.Content;

            if (model.TemplateFile != null && model.TemplateFile.Length > 0)
            {
                // Удаляем старый файл
                if (!string.IsNullOrEmpty(template.TemplatePath))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, template.TemplatePath.TrimStart('/'));
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "templates");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{Guid.NewGuid()}_{model.TemplateFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.TemplateFile.CopyToAsync(stream);
                }
                
                template.TemplatePath = $"/templates/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var template = await _context.DocumentTemplates.FindAsync(id);
            if (template == null) return;

            if (!string.IsNullOrEmpty(template.TemplatePath))
            {
                var filePath = Path.Combine(_environment.WebRootPath, template.TemplatePath.TrimStart('/'));
                if (File.Exists(filePath)) File.Delete(filePath);
            }

            _context.DocumentTemplates.Remove(template);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(int id)
        {
            var template = await _context.DocumentTemplates.FindAsync(id);
            if (template == null) return;

            template.IsActive = !template.IsActive;
            await _context.SaveChangesAsync();
        }
    }
}
