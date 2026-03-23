using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Data.Entities;

public partial class OnlineLearningSystemDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Seeding moved to SQL script SeedData.sql
    }
}
