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
using Npgsql;

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
        // Get DATABASE_URL from environment variable
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        Log.Information("Checking database config");
        
        if (string.IsNullOrEmpty(databaseUrl))
        {
            Log.Error("DATABASE_URL environment variable is not set");
            throw new InvalidOperationException("DATABASE_URL environment variable is required in production");
        }

        // Check different URL formats
        if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
        {
            try
            {
                Log.Information("Parsing PostgreSQL URL format...");
                
                // Parse URI format: postgres://username:password@host:port/database
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? userInfo[0] : "";
                var password = userInfo.Length > 1 ? userInfo[1] : "";
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.AbsolutePath.TrimStart('/');

                // Log the extracted connection details for debugging (excluding password)
                Log.Information("PostgreSQL connection details extracted: User={Username}, Host={Host}, Port={Port}, Database={Database}", 
                    username, host, port, database);
                
                // Create Npgsql connection string
                var npgsqlBuilder = new NpgsqlConnectionStringBuilder
                {
                    Host = host,
                    Port = port,
                    Database = database,
                    Username = username,
                    Password = password,
                    SslMode = SslMode.Prefer,
                    Pooling = true,
                    MinPoolSize = 1,
                    MaxPoolSize = 20,
                    ConnectionIdleLifetime = 300,
                    Timeout = 30
                };

                connectionString = npgsqlBuilder.ToString();
                Log.Information("Successfully built Npgsql connection string");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to parse PostgreSQL URL format");
                throw;
            }
        }
        else if (databaseUrl.Contains("="))
        {
            // This is already in connection string format
            Log.Information("Using provided connection string directly");
            connectionString = databaseUrl;
        }
        else
        {
            // Not a recognized format - this might be a hash or some other identifier
            Log.Error("Invalid DATABASE_URL format: {DatabaseUrlShort}...", 
                databaseUrl.Length > 10 ? databaseUrl.Substring(0, 10) : databaseUrl);
            throw new FormatException("DATABASE_URL must be a valid PostgreSQL URL or connection string");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to configure database connection string");
        throw;
    }
}

// Enable PostgreSQL-specific settings for date handling
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        Log.Information("Configuring production database with Npgsql...");
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            
            // Configure migrations table name for consistency
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
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

// Tạo các bảng và migrate database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Tạo kết nối tới database
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        Log.Information("Checking and creating database tables if needed...");
        
        // Tạo schema nếu chưa tồn tại
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE SCHEMA IF NOT EXISTS public;";
            command.ExecuteNonQuery();
            Log.Information("Public schema created or already exists.");
        }
        
        // Tạo các bảng Identity thủ công
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
-- AspNetRoles
CREATE TABLE IF NOT EXISTS ""AspNetRoles"" (
    ""Id"" text NOT NULL,
    ""Name"" character varying(256) NULL,
    ""NormalizedName"" character varying(256) NULL,
    ""ConcurrencyStamp"" text NULL,
    CONSTRAINT ""PK_AspNetRoles"" PRIMARY KEY (""Id"")
);

-- AspNetUsers
CREATE TABLE IF NOT EXISTS ""AspNetUsers"" (
    ""Id"" text NOT NULL,
    ""UserName"" character varying(256) NULL,
    ""NormalizedUserName"" character varying(256) NULL,
    ""Email"" character varying(256) NULL,
    ""NormalizedEmail"" character varying(256) NULL,
    ""EmailConfirmed"" boolean NOT NULL,
    ""PasswordHash"" text NULL,
    ""SecurityStamp"" text NULL,
    ""ConcurrencyStamp"" text NULL,
    ""PhoneNumber"" text NULL,
    ""PhoneNumberConfirmed"" boolean NOT NULL,
    ""TwoFactorEnabled"" boolean NOT NULL,
    ""LockoutEnd"" timestamp with time zone NULL,
    ""LockoutEnabled"" boolean NOT NULL,
    ""AccessFailedCount"" integer NOT NULL,
    CONSTRAINT ""PK_AspNetUsers"" PRIMARY KEY (""Id"")
);

-- AspNetRoleClaims
CREATE TABLE IF NOT EXISTS ""AspNetRoleClaims"" (
    ""Id"" serial NOT NULL,
    ""RoleId"" text NOT NULL,
    ""ClaimType"" text NULL,
    ""ClaimValue"" text NULL,
    CONSTRAINT ""PK_AspNetRoleClaims"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId"" FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE
);

-- AspNetUserClaims
CREATE TABLE IF NOT EXISTS ""AspNetUserClaims"" (
    ""Id"" serial NOT NULL,
    ""UserId"" text NOT NULL,
    ""ClaimType"" text NULL,
    ""ClaimValue"" text NULL,
    CONSTRAINT ""PK_AspNetUserClaims"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_AspNetUserClaims_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
);

-- AspNetUserLogins
CREATE TABLE IF NOT EXISTS ""AspNetUserLogins"" (
    ""LoginProvider"" text NOT NULL,
    ""ProviderKey"" text NOT NULL,
    ""ProviderDisplayName"" text NULL,
    ""UserId"" text NOT NULL,
    CONSTRAINT ""PK_AspNetUserLogins"" PRIMARY KEY (""LoginProvider"", ""ProviderKey""),
    CONSTRAINT ""FK_AspNetUserLogins_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
);

-- AspNetUserRoles
CREATE TABLE IF NOT EXISTS ""AspNetUserRoles"" (
    ""UserId"" text NOT NULL,
    ""RoleId"" text NOT NULL,
    CONSTRAINT ""PK_AspNetUserRoles"" PRIMARY KEY (""UserId"", ""RoleId""),
    CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId"" FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE,
    CONSTRAINT ""FK_AspNetUserRoles_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
);

-- AspNetUserTokens
CREATE TABLE IF NOT EXISTS ""AspNetUserTokens"" (
    ""UserId"" text NOT NULL,
    ""LoginProvider"" text NOT NULL,
    ""Name"" text NOT NULL,
    ""Value"" text NULL,
    CONSTRAINT ""PK_AspNetUserTokens"" PRIMARY KEY (""UserId"", ""LoginProvider"", ""Name""),
    CONSTRAINT ""FK_AspNetUserTokens_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
);

-- Indexes for AspNetRoles
CREATE INDEX IF NOT EXISTS ""RoleNameIndex"" ON ""AspNetRoles"" (""NormalizedName"");

-- Indexes for AspNetUsers
CREATE INDEX IF NOT EXISTS ""EmailIndex"" ON ""AspNetUsers"" (""NormalizedEmail"");
CREATE UNIQUE INDEX IF NOT EXISTS ""UserNameIndex"" ON ""AspNetUsers"" (""NormalizedUserName"");

-- Indexes for AspNetRoleClaims
CREATE INDEX IF NOT EXISTS ""IX_AspNetRoleClaims_RoleId"" ON ""AspNetRoleClaims"" (""RoleId"");

-- Indexes for AspNetUserClaims
CREATE INDEX IF NOT EXISTS ""IX_AspNetUserClaims_UserId"" ON ""AspNetUserClaims"" (""UserId"");

-- Indexes for AspNetUserLogins
CREATE INDEX IF NOT EXISTS ""IX_AspNetUserLogins_UserId"" ON ""AspNetUserLogins"" (""UserId"");

-- Indexes for AspNetUserRoles
CREATE INDEX IF NOT EXISTS ""IX_AspNetUserRoles_RoleId"" ON ""AspNetUserRoles"" (""RoleId"");

-- Categories table
CREATE TABLE IF NOT EXISTS ""Categories"" (
    ""Id"" serial NOT NULL,
    ""Name"" text NOT NULL,
    ""Description"" text NULL,
    ""ImageUrl"" text NULL,
    ""DisplayOrder"" integer NOT NULL DEFAULT 0,
    ""IsActive"" boolean NOT NULL DEFAULT true,
    CONSTRAINT ""PK_Categories"" PRIMARY KEY (""Id"")
);
";
            command.ExecuteNonQuery();
            Log.Information("Identity tables created successfully.");
        }
        
        // Kiểm tra và tạo dữ liệu Categories nếu chưa tồn tại
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM \"Categories\"";
            var count = Convert.ToInt32(command.ExecuteScalar());
            
            if (count == 0)
            {
                using (var insertCommand = connection.CreateCommand())
                {
                    insertCommand.CommandText = @"
INSERT INTO ""Categories"" (""Name"", ""Description"", ""DisplayOrder"", ""IsActive"")
VALUES 
('Trứng vịt lộn', 'Các loại trứng vịt lộn thượng hạng', 1, true),
('Món ăn kèm', 'Các món ăn kèm đặc biệt', 2, true),
('Combo', 'Các combo ưu đãi', 3, true),
('Đồ uống', 'Các loại đồ uống giải khát', 4, true);";
                    insertCommand.ExecuteNonQuery();
                    Log.Information("Categories data seeded successfully.");
                }
            }
            else
            {
                Log.Information($"Found {count} existing categories, skipping seed.");
            }
        }
        
        // Kiểm tra và tạo role mặc định
        string[] roleNames = { "Admin", "Manager", "Customer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Log.Information($"Role {roleName} created successfully.");
            }
        }
        
        // Tạo tài khoản Admin mặc định nếu chưa có
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true,
                PhoneNumber = "0123456789",
                PhoneNumberConfirmed = true
            };
            
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                Log.Information("Default admin user created successfully.");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Log.Error($"Error creating admin user: {errors}");
            }
        }
    }
    catch (Exception ex)
    {
        var npgsqlException = ex.InnerException as Npgsql.PostgresException;
        if (npgsqlException != null)
        {
            Log.Error($"PostgreSQL error: {npgsqlException.Message}");
        }
        else
        {
            Log.Error(ex, "An error occurred during database initialization.");
        }
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

if (app.Environment.IsProduction())
{
    try
    {
        Log.Information("Checking for database migrations...");
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Only check connection, don't run migrations (handled by startup script)
        try
        {
            Log.Information("Testing database connection...");
            dbContext.Database.OpenConnection();
            Log.Information("Database connection successful");
            dbContext.Database.CloseConnection();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to connect to database");
            
            if (ex is Npgsql.PostgresException pgEx)
            {
                Log.Error("PostgreSQL error: {ErrorMessage}, Code: {ErrorCode}, Detail: {ErrorDetail}", 
                    pgEx.MessageText, pgEx.SqlState, pgEx.Detail);
            }
            
            // Continue anyway, as our custom script may fix this
            Log.Warning("Continuing despite database connection issue");
        }
        
        // Don't run migrations here, they are handled by our startup script
        Log.Information("Database migrations are managed by startup script");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while checking database");
        
        // Log but don't throw, as our custom script may fix this
        Log.Warning("Continuing despite database error");
    }
}

app.Run();
