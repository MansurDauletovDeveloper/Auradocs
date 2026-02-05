using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Делегирование полномочий (заместитель)
    /// </summary>
    public class UserDelegation
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Пользователь, который делегирует полномочия
        /// </summary>
        [Required]
        public string FromUserId { get; set; } = string.Empty;

        /// <summary>
        /// Пользователь, которому делегируются полномочия (заместитель)
        /// </summary>
        [Required]
        public string ToUserId { get; set; } = string.Empty;

        /// <summary>
        /// Дата начала делегирования
        /// </summary>
        [Required]
        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания делегирования
        /// </summary>
        [Required]
        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Причина делегирования
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Причина")]
        public string? Reason { get; set; }

        /// <summary>
        /// Тип делегирования
        /// </summary>
        [Required]
        [Display(Name = "Тип делегирования")]
        public DelegationType DelegationType { get; set; } = DelegationType.Full;

        /// <summary>
        /// Активно ли делегирование
        /// </summary>
        [Display(Name = "Активно")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Кем создано
        /// </summary>
        public string? CreatedById { get; set; }

        // Навигационные свойства
        [ForeignKey("FromUserId")]
        public virtual ApplicationUser? FromUser { get; set; }

        [ForeignKey("ToUserId")]
        public virtual ApplicationUser? ToUser { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        /// <summary>
        /// Проверка, активно ли делегирование на текущий момент
        /// </summary>
        public bool IsCurrentlyActive => IsActive && 
            DateTime.UtcNow >= StartDate && 
            DateTime.UtcNow <= EndDate;
    }

    /// <summary>
    /// Тип делегирования полномочий
    /// </summary>
    public enum DelegationType
    {
        /// <summary>
        /// Полное делегирование всех полномочий
        /// </summary>
        [Display(Name = "Полное")]
        Full = 0,

        /// <summary>
        /// Только утверждение документов
        /// </summary>
        [Display(Name = "Только утверждение")]
        ApprovalOnly = 1,

        /// <summary>
        /// Только просмотр
        /// </summary>
        [Display(Name = "Только просмотр")]
        ViewOnly = 2,

        /// <summary>
        /// Для конкретного отдела
        /// </summary>
        [Display(Name = "По отделу")]
        DepartmentSpecific = 3
    }
}
