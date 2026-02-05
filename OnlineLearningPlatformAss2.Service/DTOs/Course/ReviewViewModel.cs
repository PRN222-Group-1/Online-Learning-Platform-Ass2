namespace OnlineLearningPlatformAss2.Service.DTOs.Course;

public class ReviewViewModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubmitReviewDto
{
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
