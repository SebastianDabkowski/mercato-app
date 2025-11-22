using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SD.Mercato.Reviews.Data;

/// <summary>
/// Factory for creating ReviewsDbContext instances at design time (for migrations).
/// </summary>
public class ReviewsDbContextFactory : IDesignTimeDbContextFactory<ReviewsDbContext>
{
    public ReviewsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReviewsDbContext>();
        
        // Use a connection string for design-time operations
        // This is only used for generating migrations, not at runtime
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MercatoDB;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new ReviewsDbContext(optionsBuilder.Options);
    }
}
