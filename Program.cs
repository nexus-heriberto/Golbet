
using System.Globalization;
using System.IO;
using Golbet.Data;
using Golbet.Entities;
using Golbet.Implementations;
using Golbet.Interfaces;
using Golbet.Mapping;
using Golbet.Services.Implementations;
using Golbet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Locate the project root independently of the execution directory.
static string? FindProjectRoot(string startPath)
{
    DirectoryInfo? directory = new DirectoryInfo(startPath);

    while (directory is not null)
    {
        if (File.Exists(
            Path.Combine(directory.FullName, "Golbet.csproj")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return null;
}

var projectRoot =
    FindProjectRoot(Directory.GetCurrentDirectory()) ??
    FindProjectRoot(AppContext.BaseDirectory);

if (projectRoot is null)
{
    throw new DirectoryNotFoundException(
        "No se encontró el archivo Golbet.csproj.");
}

// Configure the project root and physical folders.
Directory.SetCurrentDirectory(projectRoot);

var webRoot = Path.Combine(projectRoot, "wwwroot");
var logsDirectory = Path.Combine(projectRoot, "Logs");

Directory.CreateDirectory(logsDirectory);

if (!Directory.Exists(webRoot))
{
    throw new DirectoryNotFoundException(
        $"No se encontró la carpeta pública: {webRoot}");
}

// Configure Colombian culture.
var culture = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Create the application builder.
var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = projectRoot,
        WebRootPath = webRoot
    });

// Serilog reads its configuration from appsettings.json.
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// MVC.
builder.Services.AddControllersWithViews();

// Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity.
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Login and access-denied paths.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Generic repository.
builder.Services.AddScoped(
    typeof(IGenericRepository<>),
    typeof(GenericRepository<>));

// Match repository.
builder.Services.AddScoped<IMatchRepository, MatchRepository>();

// Bet repository.
builder.Services.AddScoped<IBetRepository, BetRepository>();

// AutoMapper.
builder.Services.AddAutoMapper(
    cfg => { },
    typeof(MappingProfile).Assembly);

// Application services.
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IBetService, BetService>();

var app = builder.Build();

// Log application startup.
app.Logger.LogInformation("Starting GolBet application");

// Initialize database, roles and administrator.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<AppUser>>();

    await DbSeeder.SeedAsync(context, roleManager, userManager);
}

// Custom exception handling in production.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/General");
}

// Custom HTTP status code pages.
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// HTTP request pipeline.
app.UseHttpsRedirection();

app.UseStaticFiles();

// Structured HTTP request logging.
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// Main MVC route.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Matches}/{action=Index}/{id?}");

// Existing development template endpoint.
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Logger.LogInformation("GolBet application is ready");

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
