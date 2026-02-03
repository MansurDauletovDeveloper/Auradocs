using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using System.Security.Claims;

namespace DocumentFlow.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class TemplatesController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly IAuditService _auditService;

        public TemplatesController(ITemplateService templateService, IAuditService auditService)
        {
            _templateService = templateService;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _templateService.GetAllTemplatesAsync();
            var viewModels = templates.Select(t => new TemplateListViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DocumentType = t.DocumentType,
                IsActive = t.IsActive,
                HasFile = !string.IsNullOrEmpty(t.TemplatePath),
                CreatedAt = t.CreatedAt
            }).ToList();

            return View(viewModels);
        }

        public IActionResult Create()
        {
            return View(new TemplateCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TemplateCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _templateService.CreateAsync(model, userId);
            
            TempData["SuccessMessage"] = "Шаблон успешно создан";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
                return NotFound();

            var model = new TemplateEditViewModel
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                DocumentType = template.DocumentType,
                Content = template.Content,
                CurrentFilePath = template.TemplatePath,
                IsActive = template.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TemplateEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            await _templateService.UpdateAsync(id, model);
            
            TempData["SuccessMessage"] = "Шаблон обновлён";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _templateService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Шаблон удалён";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            await _templateService.ToggleActiveAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetTemplateContent(int id)
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
                return NotFound();

            return Json(new { content = template.Content, filePath = template.TemplatePath });
        }
    }
}
