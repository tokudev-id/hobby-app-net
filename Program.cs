
using HobbyApp.Infrastructure.Persistence;
using HobbyApp.Infrastructure.Repositories.Implementations;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using HobbyApp.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using System.IO;
using FluentValidation;
using HobbyApp.Application.Services.Base.User;
using HobbyApp.Application.Services.Command.User;
using HobbyApp.Application.Services.Query.User;
using HobbyApp.Application.Services.Base.Authorization;
using HobbyApp.Application.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllersWithViews();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/src/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/src/Views/Shared/{0}.cshtml");
});

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Add repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
builder.Services.AddScoped(typeof(IHobbyRepository), typeof(HobbyRepository));
builder.Services.AddScoped(typeof(IRoleRepository), typeof(RoleRepository));
builder.Services.AddScoped(typeof(IUserRoleRepository), typeof(UserRoleRepository));

// Add application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Add migration and seeding services
builder.Services.AddScoped<DatabaseMigrationService>();
builder.Services.AddScoped<DatabaseSeederService>();

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandDtoValidator>();

// Add AntiForgery for CSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = false; // Allow JavaScript to read the CSRF token
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.None; // Allow HTTP in development
    options.Cookie.SameSite = SameSiteMode.Strict;
});



// JWT Authentication
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSection.GetValue<string>("SecretKey");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");
var jwtExpiryMinutes = jwtSection.GetValue<int?>("ExpiryMinutes") ?? 60;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? string.Empty))
    };
});

// JwtTokenService DI
builder.Services.AddSingleton(new JwtTokenService(
    jwtSecret ?? string.Empty,
    jwtIssuer ?? string.Empty,
    jwtAudience ?? string.Empty,
    jwtExpiryMinutes
));

// Add API controllers
builder.Services.AddControllers();



// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Run database migrations and seeding
using (var scope = app.Services.CreateScope())
{
    try
    {
        var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
        var seederService = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();

        // Migrate database (creates database if it doesn't exist)
        await migrationService.MigrateAsync();

        // Seed initial data
        await seederService.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Serve static files from src/wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "src", "wwwroot"))
});

app.UseRouting();

// Use CORS
app.UseCors("AllowAll");

// Use Authentication first
app.UseAuthentication();

app.UseMiddleware<HobbyApp.Infrastructure.Middleware.JwtAuthenticationMiddleware>();


app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

