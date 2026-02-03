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
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
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
    // Create roles
    string[] roleNames = { "Administrator", "Manager", "Employee" };
    foreach (var roleName in roleNames)
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
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
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
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(managerUser, "Manager123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(managerUser, "Manager");
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
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(employeeUser, "Employee123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(employeeUser, "Employee");
        }
    }
}
