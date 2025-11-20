using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SD.Mercato.Users.Models;

namespace SD.Mercato.Users.Data;

/// <summary>
/// Database context for the Users module.
/// Manages authentication, authorization, and user-related entities.
/// </summary>
public class UsersDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Seller staff members associated with stores.
    /// </summary>
    public DbSet<SellerStaff> SellerStaff { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure schema name for users module tables
        builder.HasDefaultSchema("users");

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.ExternalProvider)
                .HasMaxLength(50);

            entity.Property(u => u.ExternalProviderId)
                .HasMaxLength(255);

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });

        // Configure ApplicationRole
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.Description)
                .HasMaxLength(500);
        });

        // Configure SellerStaff
        builder.Entity<SellerStaff>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.JobTitle)
                .HasMaxLength(100);

            entity.HasIndex(s => new { s.UserId, s.StoreId })
                .IsUnique();

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Note: StoreId is a foreign key to the Store entity in the SellerPanel module
            // The actual FK constraint will be added at the database level or when modules are integrated
        });
    }
}
