using OnlineLearningPlatformAss2.Service.Results;
using OnlineLearningPlatformAss2.Service.DTOs.Assessment;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IAssessmentService
{
    Task<List<DTOs.Assessment.AssessmentQuestion>> GetAssessmentQuestionsAsync();
    Task<ServiceResult<Guid>> SubmitAssessmentAsync(Guid userId, Dictionary<Guid, Guid> answers);
    Task<DTOs.Assessment.AssessmentResult?> GetAssessmentResultAsync(Guid assessmentId);
    Task<List<DTOs.Assessment.CourseRecommendation>> GetRecommendationsAsync(Guid userId);
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
