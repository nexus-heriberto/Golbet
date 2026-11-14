
using System.Globalization;
using System.IO;
using Golbet.Data;
using Golbet.Entities;
using Golbet.Implementations;
using Golbet.Interfaces;
using Golbet.Mapping;
using Golbet.Services.Implementations;
using Golbet.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

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

// Configure the physical folders.
var webRoot = Path.Combine(projectRoot, "wwwroot");
var logsDirectory = Path.Combine(projectRoot, "Logs");

// Create the log directory if it does not exist.
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

// Serilog: console and daily log files.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logsDirectory, "golbet-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting GolBet application");

    // Set the correct project and web root directories.
    var builder = WebApplication.CreateBuilder(
        new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = projectRoot,
            WebRootPath = webRoot
        });

    // Serilog integration.
    builder.Host.UseSerilog();

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

    // Friendly exception page and error logging.
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature?.Error is Exception exception)
            {
                Log.Error(
                    exception,
                    "Unhandled exception for request {Path}",
                    exceptionFeature.Path);
            }

            context.Response.Clear();
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync("""
                <!DOCTYPE html>
                <html lang="es">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>GolBet - Error</title>
                </head>
                <body style="font-family:Arial;text-align:center;margin:80px;">
                    <h1>Algo salió mal</h1>
                    <p>Ocurrió un error inesperado al procesar tu solicitud.</p>
                    <p>El equipo técnico puede revisar el registro del sistema.</p>
                    <a href="/Matches">Volver a la cartelera</a>
                </body>
                </html>
                """);
        });
    });

    // Log every HTTP request.
    app.UseSerilogRequestLogging();

    // Friendly HTTP status code pages.
    app.UseStatusCodePages(async statusCodeContext =>
    {
        var response = statusCodeContext.HttpContext.Response;
        var status = response.StatusCode;

        var message = status switch
        {
            403 => "No tienes permisos para acceder a este recurso.",
            404 => "La página que buscas no está disponible.",
            _ => "No fue posible completar tu solicitud."
        };

        response.ContentType = "text/html; charset=utf-8";

        var html = $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>GolBet - Error {status}</title>
            </head>
            <body style="font-family:Arial;text-align:center;margin:80px;">
                <h1>Error {status}</h1>
                <p>{message}</p>
                <a href="/Matches">Volver a la cartelera</a>
            </body>
            </html>
            """;

        await response.WriteAsync(html);
    });

    // HTTP request pipeline.
    app.UseHttpsRedirection();

    app.UseStaticFiles();

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

    Log.Information("GolBet application is ready");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "GolBet application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
