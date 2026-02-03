using System.ComponentModel.DataAnnotations;
using DocumentFlow.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DocumentFlow.Models.ViewModels
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public AuditActionType ActionType { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DocumentId { get; set; }
        public string? DocumentTitle { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AuditLogSearchViewModel
    {
        [Display(Name = "Пользователь")]
        public string? UserId { get; set; }

        [Display(Name = "Тип действия")]
        public AuditActionType? ActionType { get; set; }

        [Display(Name = "Документ")]
        public int? DocumentId { get; set; }

        [Display(Name = "Дата с")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Дата по")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        public List<AuditLogViewModel> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class AuditLogListViewModel
    {
        public List<AuditLogViewModel> Items { get; set; } = new();
        public string? UserId { get; set; }
        public AuditActionType? ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<SelectListItem> Users { get; set; } = new();
    }
}
