using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.Services;
using QuanVitLonManager.Hubs;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using System.Text.Json;
using System.Security.Claims;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsProduction())
{
    try
    {
        // Lấy DATABASE_URL từ biến môi trường
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        Log.Information("Checking database configuration...");

        if (!string.IsNullOrEmpty(databaseUrl))
        {
            // Kiểm tra xem có phải là Postgres URL không
            if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
            {
                try
                {
                    // Parse connection string từ DATABASE_URL
                    databaseUrl = databaseUrl.Replace("postgres://", "").Replace("postgresql://", "");
                    
                    // Kiểm tra format hợp lệ
                    if (databaseUrl.Contains("@") && databaseUrl.Contains("/"))
                    {
                        var pgUserPass = databaseUrl.Split("@")[0];
                        var pgHostDb = databaseUrl.Split("@")[1];
                        
                        if (pgUserPass.Contains(":") && pgHostDb.Contains("/"))
                        {
                            var pgUserName = pgUserPass.Split(":")[0];
                            var pgPassword = pgUserPass.Split(":")[1];
                            var pgHost = pgHostDb.Split("/")[0];
                            var pgDb = pgHostDb.Split("/")[1];
                            var pgPort = pgHost.Contains(":") ? pgHost.Split(":")[1] : "5432";
                            pgHost = pgHost.Split(":")[0];

                            connectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUserName};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
                            Log.Information("Successfully configured PostgreSQL connection");
                        }
                        else
                        {
                            throw new Exception("Invalid database URL format");
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid database URL format");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error parsing PostgreSQL URL");
                    throw;
                }
            }
            else
            {
                // Nếu không phải Postgres URL, sử dụng trực tiếp
                Log.Information("Using provided connection string directly");
                connectionString = databaseUrl;
            }
        }
        else
        {
            Log.Warning("No database connection string provided");
            throw new Exception("Database connection string is required");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database configuration error");
        throw;
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        Log.Information("Configuring production database...");
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });
    }
    else
    {
        Log.Information("Configuring development database...");
        options.UseSqlServer(connectionString);
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add Google authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.Scope.Add("profile");
        options.Scope.Add("email");
    })
    .AddOAuth("Zalo", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Zalo:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Zalo:ClientSecret"];
        options.CallbackPath = "/signin-zalo";
        
        options.AuthorizationEndpoint = "https://oauth.zaloapp.com/v4/permission";
        options.TokenEndpoint = "https://oauth.zaloapp.com/v4/access_token";
        options.UserInformationEndpoint = "https://graph.zalo.me/v2.0/me";
        
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey("picture", "picture");
        
        options.SaveTokens = true;
        
        options.Events.OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
            var response = await context.Backchannel.SendAsync(request);
            var user = await response.Content.ReadFromJsonAsync<JsonElement>();
            context.RunClaimActions(user);
        };
    });

// Add core services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Add custom services
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddHttpContextAccessor();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add basic health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapControllerRoute(
    name: "chef",
    pattern: "chef/{action=Index}/{id?}",
    defaults: new { controller = "Chef" });

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Log.Information("Starting database migration...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Log connection string (ẩn password)
        var sanitizedConnectionString = connectionString?.Replace(
            context.Database.GetConnectionString() ?? "",
            "[HIDDEN]"
        );
        Log.Information($"Using connection string: {sanitizedConnectionString}");
        
        // Tự động apply migrations
        context.Database.Migrate();
        Log.Information("Database migration completed successfully");
        
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await DbInitializer.Initialize(context, userManager, roleManager);
        Log.Information("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or initializing the database");
        throw;
    }
}

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.Redirect("/Home/Error?code=404");
    }
    else if (response.StatusCode == 500)
    {
        response.Redirect("/Home/Error?code=500");
    }
});

// Add health check endpoint with basic configuration
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();
