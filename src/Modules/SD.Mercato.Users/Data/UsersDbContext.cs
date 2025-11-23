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

    /// <summary>
    /// API tokens for partner integrations.
    /// </summary>
    public DbSet<ApiToken> ApiTokens { get; set; } = null!;

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

        // Configure ApiToken
        builder.Entity<ApiToken>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.TokenHash)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.Permissions)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(a => a.IpWhitelist)
                .HasMaxLength(500);

            entity.Property(a => a.Notes)
                .HasMaxLength(1000);

            entity.HasIndex(a => a.TokenHash)
                .IsUnique();

            entity.HasIndex(a => new { a.UserId, a.IsActive });

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
