using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatformAss2.Data.Database.Entities;

public class CourseReview
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [MaxLength(1000)]
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
