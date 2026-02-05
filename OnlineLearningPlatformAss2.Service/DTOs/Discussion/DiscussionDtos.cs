namespace OnlineLearningPlatformAss2.Service.DTOs.Discussion;

public class CommentViewModel
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsInstructor { get; set; }
    public List<CommentViewModel> Replies { get; set; } = new();
}

public class CommentRequest
{
    public Guid LessonId { get; set; }
    public string Content { get; set; } = null!;
    public Guid? ParentId { get; set; }
}
