using OnlineLearningPlatformAss2.Data.Database.Entities;

namespace OnlineLearningPlatformAss2.Data.Database.EntityConfigurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        // Properties
        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.AvatarUrl)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(1000);

        // One-to-One: Profile -> User
        builder.HasOne(p => p.User)        // Profile có một User (Dùng p.User thay vì HasOne<User>)
            .WithOne(u => u.Profile)      // User cũng có một Profile
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indices
        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}
