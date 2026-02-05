namespace OnlineLearningPlatformAss2.Service.DTOs.Assessment;

public class AssessmentQuestion
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!;
    public int OrderIndex { get; set; }
    public List<AssessmentOption> Options { get; set; } = new();
}

public class AssessmentOption
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = null!;
    public string SkillLevel { get; set; } = null!;
    public string Category { get; set; } = null!;
}

public class AssessmentResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CompletedAt { get; set; }
    public Dictionary<string, string> SkillLevels { get; set; } = new();
    public List<string> RecommendedCategories { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class CourseRecommendation
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int MatchScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
}
