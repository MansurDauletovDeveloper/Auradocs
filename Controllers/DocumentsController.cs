using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DocumentFlow.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDocumentService _documentService;
        private readonly IDocumentVersionService _versionService;
        private readonly IApprovalService _approvalService;
        private readonly ICommentService _commentService;
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;

        public DocumentsController(
            UserManager<ApplicationUser> userManager,
            IDocumentService documentService,
            IDocumentVersionService versionService,
            IApprovalService approvalService,
            ICommentService commentService,
            IAuditService auditService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _documentService = documentService;
            _versionService = versionService;
            _approvalService = approvalService;
            _commentService = commentService;
            _auditService = auditService;
            _context = context;
        }

        public async Task<IActionResult> Index(DocumentSearchViewModel search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            var (documents, totalCount) = await _documentService.SearchDocumentsAsync(search, user.Id, isAdmin);

            search.Documents = documents;
            search.TotalCount = totalCount;

            await PopulateSearchFilters();

            return View(search);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateTemplates();
            return View(new DocumentCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTemplates();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            try
            {
                var document = await _documentService.CreateDocumentAsync(model, user.Id);
                TempData["SuccessMessage"] = "Документ успешно создан.";
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateTemplates();
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            
            if (!await _documentService.CanUserAccessDocumentAsync(id, user.Id, isAdmin))
                return Forbid();

            var model = await _documentService.GetDocumentDetailsAsync(id, user.Id);
            if (model == null) return NotFound();

            // Логируем просмотр
            await _auditService.LogAsync(user.Id, AuditActionType.View, 
                $"Просмотр документа: {model.Title}", id);

            // Получаем список руководителей для согласования
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            var admins = await _userManager.GetUsersInRoleAsync("Administrator");
            ViewBag.Approvers = managers.Union(admins)
                .Where(u => u.Id != user.Id && u.IsActive)
                .Select(u => new SelectListItem(u.FullName, u.Id))
                .ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!await _documentService.CanUserEditDocumentAsync(id, user.Id))
                return Forbid();

            var document = await _documentService.GetDocumentAsync(id);
            if (document == null) return NotFound();

            var currentVersion = document.Versions.FirstOrDefault(v => v.IsCurrent);

            var model = new DocumentEditViewModel
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                DocumentType = document.DocumentType,
                Content = currentVersion?.Content,
                Status = document.Status,
                CurrentVersion = document.CurrentVersion,
                AuthorName = document.Author?.FullName,
                CreatedAt = document.CreatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DocumentEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            try
            {
                var result = await _documentService.UpdateDocumentAsync(model, user.Id);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Не удалось обновить документ.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Документ успешно обновлён.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _documentService.DeleteDocumentAsync(id, user.Id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Не удалось удалить документ.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Документ успешно удалён.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApproval(SubmitForApprovalViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (model.ApproverIds == null || !model.ApproverIds.Any())
            {
                TempData["ErrorMessage"] = "Выберите хотя бы одного согласующего.";
                return RedirectToAction(nameof(Details), new { id = model.DocumentId });
            }

            var result = await _approvalService.SubmitForApprovalAsync(
                model.DocumentId, 
                model.ApproverIds, 
                user.Id, 
                model.Comment, 
                model.DueDate);

            if (!result)
            {
                TempData["ErrorMessage"] = "Не удалось отправить документ на согласование.";
                return RedirectToAction(nameof(Details), new { id = model.DocumentId });
            }

            TempData["SuccessMessage"] = "Документ отправлен на согласование.";
            return RedirectToAction(nameof(Details), new { id = model.DocumentId });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadVersion(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var version = await _versionService.GetVersionAsync(id);
            if (version == null) return NotFound();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            if (!await _documentService.CanUserAccessDocumentAsync(version.DocumentId, user.Id, isAdmin))
                return Forbid();

            var fileContent = await _versionService.DownloadVersionAsync(id, user.Id);
            if (fileContent == null) return NotFound();

            var contentType = version.ContentType ?? "application/octet-stream";
            return File(fileContent, contentType, version.FileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreVersion(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var version = await _versionService.GetVersionAsync(id);
            if (version == null) return NotFound();

            var result = await _versionService.RestoreVersionAsync(id, user.Id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Не удалось восстановить версию.";
                return RedirectToAction(nameof(Details), new { id = version.DocumentId });
            }

            TempData["SuccessMessage"] = "Версия успешно восстановлена.";
            return RedirectToAction(nameof(Details), new { id = version.DocumentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(AddCommentViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.Text))
            {
                TempData["ErrorMessage"] = "Введите текст комментария.";
                return RedirectToAction(nameof(Details), new { id = model.DocumentId });
            }

            await _commentService.AddCommentAsync(model.DocumentId, user.Id, model.Text, model.ParentCommentId);
            
            TempData["SuccessMessage"] = "Комментарий добавлен.";
            return RedirectToAction(nameof(Details), new { id = model.DocumentId });
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            var search = new DocumentSearchViewModel { PageSize = 10000 };
            var (documents, _) = await _documentService.SearchDocumentsAsync(search, user.Id, isAdmin);

            var csv = new StringBuilder();
            csv.AppendLine("ID;Рег. номер;Название;Тип;Статус;Автор;Дата создания;Версия");

            foreach (var doc in documents)
            {
                csv.AppendLine($"{doc.Id};{doc.RegistrationNumber};{doc.Title};{GetDocumentTypeName(doc.DocumentType)};{GetStatusName(doc.Status)};{doc.AuthorName};{doc.CreatedAt:dd.MM.yyyy};{doc.CurrentVersion}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"documents_{DateTime.Now:yyyyMMdd}.csv");
        }

        private async Task PopulateSearchFilters()
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Authors = users.Select(u => new SelectListItem(u.FullName, u.Id)).ToList();
        }

        private async Task PopulateTemplates()
        {
            var templates = await _context.DocumentTemplates.Where(t => t.IsActive).ToListAsync();
            ViewBag.Templates = templates.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
        }

        private static string GetDocumentTypeName(DocumentType type) => type switch
        {
            DocumentType.Application => "Заявление",
            DocumentType.Order => "Приказ",
            DocumentType.Contract => "Договор",
            DocumentType.Act => "Акт",
            DocumentType.Memo => "Служебная записка",
            DocumentType.Report => "Отчёт",
            DocumentType.Protocol => "Протокол",
            _ => "Другое"
        };

        private static string GetStatusName(DocumentStatus status) => status switch
        {
            DocumentStatus.Draft => "Черновик",
            DocumentStatus.Pending => "На согласовании",
            DocumentStatus.Approved => "Утверждён",
            DocumentStatus.Rejected => "Отклонён",
            DocumentStatus.Archived => "Архив",
            _ => "Неизвестно"
        };
    }
}
