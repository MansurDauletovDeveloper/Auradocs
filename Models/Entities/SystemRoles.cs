namespace DocumentFlow.Models.Entities
{
    /// <summary>
    /// Системные роли пользователей
    /// </summary>
    public static class SystemRoles
    {
        /// <summary>
        /// Администратор - полные права на систему
        /// </summary>
        public const string Administrator = "Administrator";

        /// <summary>
        /// Сотрудник - создание и редактирование собственных документов
        /// </summary>
        public const string Employee = "Employee";

        /// <summary>
        /// Руководитель - согласование документов подчинённых
        /// </summary>
        public const string Manager = "Manager";

        /// <summary>
        /// Владелец документа - ответственный за конкретный документ
        /// </summary>
        public const string DocumentOwner = "DocumentOwner";

        /// <summary>
        /// Рецензент - проверка содержательной/технической части
        /// </summary>
        public const string Reviewer = "Reviewer";

        /// <summary>
        /// Юридический отдел - проверка юридических документов
        /// </summary>
        public const string LegalDepartment = "LegalDepartment";

        /// <summary>
        /// Аудитор - просмотр журналов аудита
        /// </summary>
        public const string Auditor = "Auditor";

        /// <summary>
        /// Архивариус - управление архивом документов
        /// </summary>
        public const string Archivist = "Archivist";

        /// <summary>
        /// ИТ / Интегратор - управление интеграциями
        /// </summary>
        public const string ITIntegrator = "ITIntegrator";

        /// <summary>
        /// Внешний пользователь / Контрагент - ограниченный доступ
        /// </summary>
        public const string ExternalUser = "ExternalUser";

        /// <summary>
        /// Просмотрщик - только чтение
        /// </summary>
        public const string Viewer = "Viewer";

        /// <summary>
        /// Должностное лицо по соблюдению требований
        /// </summary>
        public const string ComplianceOfficer = "ComplianceOfficer";

        /// <summary>
        /// Заместитель - временное исполнение обязанностей
        /// </summary>
        public const string Deputy = "Deputy";

        /// <summary>
        /// Все роли системы
        /// </summary>
        public static readonly string[] AllRoles = new[]
        {
            Administrator,
            Employee,
            Manager,
            DocumentOwner,
            Reviewer,
            LegalDepartment,
            Auditor,
            Archivist,
            ITIntegrator,
            ExternalUser,
            Viewer,
            ComplianceOfficer,
            Deputy
        };

        /// <summary>
        /// Получить отображаемое имя роли
        /// </summary>
        public static string GetDisplayName(string role)
        {
            return role switch
            {
                Administrator => "Администратор",
                Employee => "Сотрудник",
                Manager => "Руководитель",
                DocumentOwner => "Владелец документа",
                Reviewer => "Рецензент",
                LegalDepartment => "Юридический отдел",
                Auditor => "Аудитор",
                Archivist => "Архивариус",
                ITIntegrator => "ИТ / Интегратор",
                ExternalUser => "Внешний пользователь",
                Viewer => "Просмотрщик",
                ComplianceOfficer => "Должностное лицо по соблюдению требований",
                Deputy => "Заместитель",
                _ => role
            };
        }

        /// <summary>
        /// Получить описание роли
        /// </summary>
        public static string GetDescription(string role)
        {
            return role switch
            {
                Administrator => "Полные права на систему: управление пользователями и ролями, конфигурация, резервное копирование/восстановление, просмотр и удаление любых документов и логов, управление интеграциями.",
                Employee => "Создаёт и редактирует собственные документы, работает с шаблонами, отправляет документы на согласование, просматривает статусы своих документов.",
                Manager => "Получает запросы на согласование от подчинённых, утверждает/отклоняет документы с комментариями, может запросить доработку.",
                DocumentOwner => "Формально ответственный за конкретный документ. Имеет права на редактирование, контроль жизненного цикла документа, делегирование согласующих.",
                Reviewer => "Проводит проверку содержательной/технической части, оставляет комментарии и пометки, может предлагать правки.",
                LegalDepartment => "Просмотр и обязательная проверка документов с юридическим содержанием; может блокировать утверждение до устранения замечаний.",
                Auditor => "Доступ только для просмотра журналов аудита, истории версий и метаданных; не может изменять документы.",
                Archivist => "Управляет архивом утверждённых документов: перемещение в хранение, восстановление из архива, управление политиками хранения.",
                ITIntegrator => "Права на управление интеграциями (API ключи, настройки LDAP/SMTP), мониторинг очередей и задач, доступ к диагностическим логам.",
                ExternalUser => "Временный/ограниченный доступ к отдельным документам. Доступ строго по приглашению с ограничениями по экспорту/печати.",
                Viewer => "Ограниченный доступ только к чтению выбранных документов/категорий без возможности скачивания.",
                ComplianceOfficer => "Контроль политик безопасности и конфиденциальности, проверка прав доступа, инициирование ревизий доступов.",
                Deputy => "Назначается временно для выполнения действий от имени руководителя с ограничением по сроку и объёму полномочий.",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Роли, которые могут утверждать документы
        /// </summary>
        public static readonly string[] ApprovalRoles = new[]
        {
            Administrator,
            Manager,
            DocumentOwner,
            LegalDepartment,
            Deputy
        };

        /// <summary>
        /// Роли, которые могут комментировать документы
        /// </summary>
        public static readonly string[] CommentRoles = new[]
        {
            Administrator,
            Employee,
            Manager,
            DocumentOwner,
            Reviewer,
            LegalDepartment,
            Auditor,
            ComplianceOfficer,
            Deputy
        };

        /// <summary>
        /// Роли с доступом к аудиту
        /// </summary>
        public static readonly string[] AuditAccessRoles = new[]
        {
            Administrator,
            Auditor,
            ComplianceOfficer
        };

        /// <summary>
        /// Роли для работы с архивом
        /// </summary>
        public static readonly string[] ArchiveRoles = new[]
        {
            Administrator,
            Archivist
        };
    }
}
