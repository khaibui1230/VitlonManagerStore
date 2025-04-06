using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QuanVitLonManager.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Always use PostgreSQL for migrations to ensure compatibility
            optionsBuilder.UseNpgsql("Host=localhost;Database=vitlondb;Username=postgres;Password=postgres", 
                o => o.MigrationsHistoryTable("__EFMigrationsHistory"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
} 