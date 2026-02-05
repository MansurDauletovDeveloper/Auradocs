using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Уровень доступа к документу
    /// </summary>
    public enum AccessLevel
    {
        /// <summary>
        /// Только просмотр
        /// </summary>
        [Display(Name = "Только просмотр")]
        View = 0,

        /// <summary>
        /// Просмотр и комментирование
        /// </summary>
        [Display(Name = "Просмотр и комментирование")]
        ViewAndComment = 1,

        /// <summary>
        /// Редактирование
        /// </summary>
        [Display(Name = "Редактирование")]
        Edit = 2,

        /// <summary>
        /// Полный доступ
        /// </summary>
        [Display(Name = "Полный доступ")]
        Full = 3
    }

    /// <summary>
    /// Доступ пользователя к конкретному документу
    /// </summary>
    public class DocumentAccess
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID документа
        /// </summary>
        [Required]
        public int DocumentId { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Уровень доступа
        /// </summary>
        [Required]
        [Display(Name = "Уровень доступа")]
        public AccessLevel AccessLevel { get; set; } = AccessLevel.View;

        /// <summary>
        /// Роль пользователя для этого документа
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Роль")]
        public string? DocumentRole { get; set; }

        /// <summary>
        /// Разрешено скачивание
        /// </summary>
        [Display(Name = "Скачивание")]
        public bool CanDownload { get; set; } = false;

        /// <summary>
        /// Разрешена печать
        /// </summary>
        [Display(Name = "Печать")]
        public bool CanPrint { get; set; } = false;

        /// <summary>
        /// Разрешён экспорт
        /// </summary>
        [Display(Name = "Экспорт")]
        public bool CanExport { get; set; } = false;

        /// <summary>
        /// Дата окончания доступа (для временного доступа)
        /// </summary>
        [Display(Name = "Доступ до")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Кем предоставлен доступ
        /// </summary>
        public string? GrantedById { get; set; }

        /// <summary>
        /// Дата предоставления доступа
        /// </summary>
        [Display(Name = "Дата предоставления")]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Комментарий к доступу
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }

        /// <summary>
        /// Активен ли доступ
        /// </summary>
        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("GrantedById")]
        public virtual ApplicationUser? GrantedBy { get; set; }

        /// <summary>
        /// Проверка, активен ли доступ на текущий момент
        /// </summary>
        public bool IsCurrentlyActive => IsActive && 
            (!ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow);
    }

    /// <summary>
    /// Блокировка документа (юридический отдел)
    /// </summary>
    public class DocumentBlock
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID документа
        /// </summary>
        [Required]
        public int DocumentId { get; set; }

        /// <summary>
        /// Кем заблокирован
        /// </summary>
        [Required]
        public string BlockedById { get; set; } = string.Empty;

        /// <summary>
        /// Причина блокировки
        /// </summary>
        [Required]
        [StringLength(1000)]
        [Display(Name = "Причина блокировки")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Тип блокировки
        /// </summary>
        [Required]
        [Display(Name = "Тип блокировки")]
        public BlockType BlockType { get; set; } = BlockType.ApprovalBlock;

        /// <summary>
        /// Дата блокировки
        /// </summary>
        [Display(Name = "Дата блокировки")]
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата снятия блокировки
        /// </summary>
        [Display(Name = "Дата снятия")]
        public DateTime? UnblockedAt { get; set; }

        /// <summary>
        /// Кем снята блокировка
        /// </summary>
        public string? UnblockedById { get; set; }

        /// <summary>
        /// Комментарий к снятию блокировки
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Комментарий к снятию")]
        public string? UnblockComment { get; set; }

        /// <summary>
        /// Активна ли блокировка
        /// </summary>
        [Display(Name = "Активна")]
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        [ForeignKey("BlockedById")]
        public virtual ApplicationUser? BlockedBy { get; set; }

        [ForeignKey("UnblockedById")]
        public virtual ApplicationUser? UnblockedBy { get; set; }
    }

    /// <summary>
    /// Тип блокировки документа
    /// </summary>
    public enum BlockType
    {
        /// <summary>
        /// Блокировка утверждения
        /// </summary>
        [Display(Name = "Блокировка утверждения")]
        ApprovalBlock = 0,

        /// <summary>
        /// Блокировка редактирования
        /// </summary>
        [Display(Name = "Блокировка редактирования")]
        EditBlock = 1,

        /// <summary>
        /// Полная блокировка
        /// </summary>
        [Display(Name = "Полная блокировка")]
        FullBlock = 2,

        /// <summary>
        /// Юридическая проверка
        /// </summary>
        [Display(Name = "Юридическая проверка")]
        LegalReview = 3,

        /// <summary>
        /// Проверка соответствия
        /// </summary>
        [Display(Name = "Проверка соответствия")]
        ComplianceReview = 4
    }
}
