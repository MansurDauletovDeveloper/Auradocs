using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings - усиленная политика паролей
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true; // Теперь требуется спец. символ
    options.Password.RequiredLength = 8; // Минимум 8 символов
    options.Password.RequiredUniqueChars = 4; // Минимум 4 уникальных символа
    
    // Lockout settings - защита от brute-force
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // SignIn settings - требуется подтверждённый email
    options.SignIn.RequireConfirmedEmail = false; // Обрабатываем вручную в контроллере
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddPasswordValidator<PasswordValidatorService>(); // Кастомный валидатор паролей

// Cookie settings - настройки безопасности сессий
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Сессия 8 часов
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true; // Защита от XSS
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS в продакшене
    options.Cookie.SameSite = SameSiteMode.Strict; // Защита от CSRF
});

// Application Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentVersionService, DocumentVersionService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

// Security Services - новые сервисы безопасности
builder.Services.AddScoped<IPasswordValidatorService, PasswordValidatorService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await context.Database.MigrateAsync();
        await SeedDataAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Seed initial data
static async Task SeedDataAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Create all roles from SystemRoles
    foreach (var roleName in SystemRoles.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create admin user
    var adminEmail = "admin@documentflow.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Администратор",
            LastName = "Системы",
            Position = "Системный администратор",
            Department = "IT",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, SystemRoles.Administrator);
        }
    }

    // Create test manager
    var managerEmail = "manager@documentflow.local";
    var managerUser = await userManager.FindByEmailAsync(managerEmail);
    
    if (managerUser == null)
    {
        managerUser = new ApplicationUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            FirstName = "Иван",
            LastName = "Петров",
            MiddleName = "Сергеевич",
            Position = "Руководитель отдела",
            Department = "Отдел разработки",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(managerUser, "Manager123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(managerUser, SystemRoles.Manager);
        }
    }

    // Create test employee
    var employeeEmail = "employee@documentflow.local";
    var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
    
    if (employeeUser == null)
    {
        employeeUser = new ApplicationUser
        {
            UserName = employeeEmail,
            Email = employeeEmail,
            FirstName = "Мария",
            LastName = "Иванова",
            MiddleName = "Александровна",
            Position = "Специалист",
            Department = "Отдел разработки",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            ManagerId = managerUser?.Id,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(employeeUser, "Employee123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(employeeUser, SystemRoles.Employee);
        }
    }

    // Create legal department user
    var legalEmail = "legal@documentflow.local";
    var legalUser = await userManager.FindByEmailAsync(legalEmail);
    
    if (legalUser == null)
    {
        legalUser = new ApplicationUser
        {
            UserName = legalEmail,
            Email = legalEmail,
            FirstName = "Елена",
            LastName = "Правова",
            MiddleName = "Юрьевна",
            Position = "Юрист",
            Department = "Юридический отдел",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(legalUser, "Legal123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(legalUser, SystemRoles.LegalDepartment);
        }
    }

    // Create auditor user
    var auditorEmail = "auditor@documentflow.local";
    var auditorUser = await userManager.FindByEmailAsync(auditorEmail);
    
    if (auditorUser == null)
    {
        auditorUser = new ApplicationUser
        {
            UserName = auditorEmail,
            Email = auditorEmail,
            FirstName = "Андрей",
            LastName = "Проверкин",
            MiddleName = "Контролевич",
            Position = "Аудитор",
            Department = "Отдел внутреннего аудита",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = false,
            CanPrint = false,
            CanDownload = false
        };

        var result = await userManager.CreateAsync(auditorUser, "Auditor123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(auditorUser, SystemRoles.Auditor);
        }
    }

    // Create archivist user
    var archivistEmail = "archivist@documentflow.local";
    var archivistUser = await userManager.FindByEmailAsync(archivistEmail);
    
    if (archivistUser == null)
    {
        archivistUser = new ApplicationUser
        {
            UserName = archivistEmail,
            Email = archivistEmail,
            FirstName = "Ольга",
            LastName = "Архивная",
            MiddleName = "Сергеевна",
            Position = "Архивариус",
            Department = "Архив",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(archivistUser, "Archivist123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(archivistUser, SystemRoles.Archivist);
        }
    }

    // Create reviewer user
    var reviewerEmail = "reviewer@documentflow.local";
    var reviewerUser = await userManager.FindByEmailAsync(reviewerEmail);
    
    if (reviewerUser == null)
    {
        reviewerUser = new ApplicationUser
        {
            UserName = reviewerEmail,
            Email = reviewerEmail,
            FirstName = "Сергей",
            LastName = "Рецензентов",
            MiddleName = "Петрович",
            Position = "Старший специалист",
            Department = "Отдел качества",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(reviewerUser, "Reviewer123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(reviewerUser, SystemRoles.Reviewer);
        }
    }

    // Create compliance officer user
    var complianceEmail = "compliance@documentflow.local";
    var complianceUser = await userManager.FindByEmailAsync(complianceEmail);
    
    if (complianceUser == null)
    {
        complianceUser = new ApplicationUser
        {
            UserName = complianceEmail,
            Email = complianceEmail,
            FirstName = "Наталья",
            LastName = "Соответствиева",
            MiddleName = "Ивановна",
            Position = "Специалист по соответствию",
            Department = "Отдел комплаенс",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(complianceUser, "Compliance123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(complianceUser, SystemRoles.ComplianceOfficer);
        }
    }

    // Create IT Integrator user
    var itEmail = "it@documentflow.local";
    var itUser = await userManager.FindByEmailAsync(itEmail);
    
    if (itUser == null)
    {
        itUser = new ApplicationUser
        {
            UserName = itEmail,
            Email = itEmail,
            FirstName = "Дмитрий",
            LastName = "Айтишников",
            MiddleName = "Сергеевич",
            Position = "ИТ-специалист",
            Department = "IT",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(itUser, "ITAdmin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(itUser, SystemRoles.ITIntegrator);
        }
    }

    // Create Document Owner user
    var docOwnerEmail = "docowner@documentflow.local";
    var docOwnerUser = await userManager.FindByEmailAsync(docOwnerEmail);
    
    if (docOwnerUser == null)
    {
        docOwnerUser = new ApplicationUser
        {
            UserName = docOwnerEmail,
            Email = docOwnerEmail,
            FirstName = "Александр",
            LastName = "Владельцев",
            MiddleName = "Петрович",
            Position = "Ведущий специалист",
            Department = "Отдел документооборота",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(docOwnerUser, "DocOwner123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(docOwnerUser, SystemRoles.DocumentOwner);
        }
    }

    // Create External User
    var externalEmail = "external@partner.com";
    var externalUser = await userManager.FindByEmailAsync(externalEmail);
    
    if (externalUser == null)
    {
        externalUser = new ApplicationUser
        {
            UserName = externalEmail,
            Email = externalEmail,
            FirstName = "Партнёр",
            LastName = "Внешний",
            Position = "Представитель контрагента",
            Department = "Внешняя компания",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = false,
            CanPrint = false,
            CanDownload = false,
            IsExternalUser = true,
            AccessExpiresAt = DateTime.UtcNow.AddMonths(3)
        };

        var result = await userManager.CreateAsync(externalUser, "External123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(externalUser, SystemRoles.ExternalUser);
        }
    }

    // Create Viewer user
    var viewerEmail = "viewer@documentflow.local";
    var viewerUser = await userManager.FindByEmailAsync(viewerEmail);
    
    if (viewerUser == null)
    {
        viewerUser = new ApplicationUser
        {
            UserName = viewerEmail,
            Email = viewerEmail,
            FirstName = "Виктор",
            LastName = "Смотрящий",
            MiddleName = "Иванович",
            Position = "Наблюдатель",
            Department = "Общий отдел",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CanExport = false,
            CanPrint = false,
            CanDownload = false
        };

        var result = await userManager.CreateAsync(viewerUser, "Viewer123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(viewerUser, SystemRoles.Viewer);
        }
    }

    // Create Deputy user
    var deputyEmail = "deputy@documentflow.local";
    var deputyUser = await userManager.FindByEmailAsync(deputyEmail);
    
    if (deputyUser == null)
    {
        deputyUser = new ApplicationUser
        {
            UserName = deputyEmail,
            Email = deputyEmail,
            FirstName = "Заместитель",
            LastName = "Временный",
            MiddleName = "Олегович",
            Position = "Заместитель руководителя",
            Department = "Отдел разработки",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            ManagerId = managerUser?.Id,
            CanExport = true,
            CanPrint = true,
            CanDownload = true
        };

        var result = await userManager.CreateAsync(deputyUser, "Deputy123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(deputyUser, SystemRoles.Deputy);
        }
    }
}
