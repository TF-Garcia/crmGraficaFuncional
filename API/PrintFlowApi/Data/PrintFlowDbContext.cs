using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Model;

namespace PrintFlowApi.Data;

public class PrintFlowDbContext(DbContextOptions<PrintFlowDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductQuantity> ProductQuantities => Set<ProductQuantity>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<OrderFile> OrderFiles => Set<OrderFile>();
    public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(product => product.Slug).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(order => order.Number).IsUnique();
        modelBuilder.Entity<Quote>().HasIndex(quote => quote.Number).IsUnique();
        modelBuilder.Entity<Payment>().HasIndex(payment => payment.ProviderReference);
        modelBuilder.Entity<PasswordResetToken>().HasIndex(token => token.TokenHash);

        modelBuilder.Entity<Product>().Property(product => product.BasePrice).HasPrecision(10, 2);
        modelBuilder.Entity<ProductOption>().Property(option => option.PriceDelta).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(order => order.Subtotal).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(order => order.UrgencyFee).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(order => order.DeliveryFee).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(order => order.Total).HasPrecision(10, 2);
        modelBuilder.Entity<Quote>().Property(quote => quote.Subtotal).HasPrecision(10, 2);
        modelBuilder.Entity<Quote>().Property(quote => quote.UrgencyFee).HasPrecision(10, 2);
        modelBuilder.Entity<Quote>().Property(quote => quote.DeliveryFee).HasPrecision(10, 2);
        modelBuilder.Entity<Quote>().Property(quote => quote.Total).HasPrecision(10, 2);
        modelBuilder.Entity<InventoryItem>().Property(item => item.Available).HasPrecision(12, 2);
        modelBuilder.Entity<InventoryItem>().Property(item => item.Minimum).HasPrecision(12, 2);
        modelBuilder.Entity<InventoryItem>().Property(item => item.UnitCost).HasPrecision(10, 2);
        modelBuilder.Entity<StockMovement>().Property(item => item.Quantity).HasPrecision(12, 2);
        modelBuilder.Entity<Payment>().Property(payment => payment.Amount).HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .HasOne(order => order.Payment)
            .WithOne(payment => payment.Order)
            .HasForeignKey<Payment>(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
