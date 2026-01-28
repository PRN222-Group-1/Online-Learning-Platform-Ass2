namespace OnlineLearningPlatformAss2.Data.Database.Entities;

public class Profile : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Foreign Key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
