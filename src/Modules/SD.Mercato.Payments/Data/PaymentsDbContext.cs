using Microsoft.EntityFrameworkCore;
using SD.Mercato.Payments.Models;

namespace SD.Mercato.Payments.Data;

/// <summary>
/// Database context for the Payments module.
/// Manages payment transactions, seller balances, payouts, and SubOrder payment tracking.
/// </summary>
public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Payment transactions (buyer payments to platform).
    /// </summary>
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    /// <summary>
    /// Seller balance tracking (pending, available, paid out amounts).
    /// </summary>
    public DbSet<SellerBalance> SellerBalances => Set<SellerBalance>();

    /// <summary>
    /// Payouts from platform to sellers.
    /// </summary>
    public DbSet<Payout> Payouts => Set<Payout>();

    /// <summary>
    /// SubOrder payment breakdown (commission, fees, net amounts per seller).
    /// </summary>
    public DbSet<SubOrderPayment> SubOrderPayments => Set<SubOrderPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PaymentTransaction configuration
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("PaymentTransactions");
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.PaymentGatewayTransactionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ProcessingFee).HasPrecision(18, 2);
        });

        // SellerBalance configuration
        modelBuilder.Entity<SellerBalance>(entity =>
        {
            entity.ToTable("SellerBalances");
            entity.HasIndex(e => e.StoreId).IsUnique();

            entity.Property(e => e.PendingAmount).HasPrecision(18, 2);
            entity.Property(e => e.AvailableAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalPaidOut).HasPrecision(18, 2);
        });

        // Payout configuration
        modelBuilder.Entity<Payout>(entity =>
        {
            entity.ToTable("Payouts");
            entity.HasIndex(e => e.StoreId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.GrossAmount).HasPrecision(18, 2);
            entity.Property(e => e.CommissionAmount).HasPrecision(18, 2);
            entity.Property(e => e.ProcessingFeeAmount).HasPrecision(18, 2);
        });

        // SubOrderPayment configuration
        modelBuilder.Entity<SubOrderPayment>(entity =>
        {
            entity.ToTable("SubOrderPayments");
            entity.HasIndex(e => e.SubOrderId);
            entity.HasIndex(e => e.PaymentTransactionId);
            entity.HasIndex(e => e.StoreId);
            entity.HasIndex(e => e.PayoutStatus);
            entity.HasIndex(e => e.PayoutId);

            entity.Property(e => e.ProductTotal).HasPrecision(18, 2);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.SubOrderTotal).HasPrecision(18, 2);
            entity.Property(e => e.CommissionRate).HasPrecision(5, 4); // e.g., 0.1500 for 15%
            entity.Property(e => e.CommissionAmount).HasPrecision(18, 2);
            entity.Property(e => e.ProcessingFeeAllocated).HasPrecision(18, 2);
            entity.Property(e => e.SellerNetAmount).HasPrecision(18, 2);

            // Configure relationship with Payout
            entity.HasOne(e => e.Payout)
                  .WithMany()
                  .HasForeignKey(e => e.PayoutId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
