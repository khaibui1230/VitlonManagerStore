using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Models;

namespace QuanVitLonManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Entity<MenuItem>()
                .Property(m => m.OriginalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<OrderDetail>()
                .Property(od => od.Subtotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            // Configure relationships and constraints with better cascade behavior
            builder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Configure OrderDetail relationships
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<OrderDetail>()
                .HasOne(od => od.MenuItem)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(od => od.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configure CartItem relationship
            builder.Entity<CartItem>()
                .HasOne(c => c.MenuItem)
                .WithMany()
                .HasForeignKey(c => c.MenuItemId);

            builder.Entity<DishOrder>()
                .Property(d => d.Price)
                .HasColumnType("decimal(18,2)");

            // Make sure DishOrder is properly configured
            builder.Entity<DishOrder>()
                .HasKey(d => d.Id);
                
            builder.Entity<DishOrder>()
                .Property(d => d.TotalPrice)
                .HasColumnType("decimal(18,2)");
        }

        // Thêm vào class ApplicationDbContext
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        // Thêm vào class ApplicationDbContext
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<DishOrder> DishOrders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<RestaurantInfo> RestaurantInfo { get; set; }
        public DbSet<UserCart> UserCarts { get; set; }
    }
}