using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;

namespace QuanVitLonManager.Data
{
    public static class DatabaseHelpers
    {
        /// <summary>
        /// Configure entity properties for PostgreSQL compatibility
        /// </summary>
        public static void ConfigureForPostgreSQL<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            // Get all string properties
            var stringProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string));

            // Configure string properties to use text for PostgreSQL instead of nvarchar
            foreach (var property in stringProperties)
            {
                builder.Property(property.Name)
                    .HasColumnType("text");
            }

            // Get all decimal properties
            var decimalProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

            // Configure decimal properties for PostgreSQL
            foreach (var property in decimalProperties)
            {
                builder.Property(property.Name)
                    .HasColumnType("numeric(18,2)");
            }
        }

        /// <summary>
        /// Configure model for PostgreSQL compatibility
        /// </summary>
        public static void ConfigurePostgreSqlCompatibility(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Convert table names to lowercase for PostgreSQL
                entity.SetTableName(entity.GetTableName().ToLower());

                // Process each property
                foreach (var property in entity.GetProperties())
                {
                    // Convert column names to lowercase
                    property.SetColumnName(property.GetColumnName().ToLower());

                    // Convert string properties from nvarchar to text
                    if (property.ClrType == typeof(string))
                    {
                        property.SetColumnType("text");
                    }
                    
                    // Handle decimal properties
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetColumnType("numeric(18,2)");
                    }
                    
                    // Handle date/time properties
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp");
                    }
                }

                // Handle indexes and keys
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToLower());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName().ToLower());
                }

                foreach (var fk in entity.GetForeignKeys())
                {
                    fk.SetConstraintName(fk.GetConstraintName().ToLower());
                }
            }
        }
    }
} 