using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.Results;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using AssessmentQuestionDto = OnlineLearningPlatformAss2.Service.DTOs.Assessment.AssessmentQuestion;
using AssessmentOptionDto = OnlineLearningPlatformAss2.Service.DTOs.Assessment.AssessmentOption;
using AssessmentResultDto = OnlineLearningPlatformAss2.Service.DTOs.Assessment.AssessmentResult;
using CourseRecommendationDto = OnlineLearningPlatformAss2.Service.DTOs.Assessment.CourseRecommendation;

namespace OnlineLearningPlatformAss2.Service.Services;

public class AssessmentService : IAssessmentService
{
    private readonly OnlineLearningContext _context;

    public AssessmentService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<List<DTOs.Assessment.AssessmentQuestion>> GetAssessmentQuestionsAsync()
    {
        try
        {
            var questions = await _context.AssessmentQuestions
                .Include(q => q.Options)
                .Where(q => q.IsActive)
                .OrderBy(q => q.OrderIndex)
                .Select(q => new AssessmentQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    OrderIndex = q.OrderIndex,
                    Options = q.Options.Select(o => new AssessmentOptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        SkillLevel = o.SkillLevel,
                        Category = o.Category
                    }).ToList()
                })
                .ToListAsync();

            // If no questions in DB, return default questions
            if (!questions.Any())
            {
                return GetDefaultQuestions();
            }

            return questions;
        }
        catch
        {
            // Return default questions if database error
            return GetDefaultQuestions();
        }
    }

    public async Task<ServiceResult<Guid>> SubmitAssessmentAsync(Guid userId, Dictionary<Guid, Guid> answers)
    {
        try
        {
            // Get the options for analysis
            var selectedOptions = await _context.AssessmentOptions
                .Where(o => answers.Values.Contains(o.Id))
                .ToListAsync();

            // Analyze results
            var skillLevels = AnalyzeSkillLevels(selectedOptions);
            var recommendedCategories = AnalyzeRecommendedCategories(selectedOptions);
            var summary = GenerateSummary(skillLevels, recommendedCategories);

            // Create assessment record
            var assessment = new UserAssessment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompletedAt = DateTime.UtcNow,
                ResultSummary = summary
            };

            _context.UserAssessments.Add(assessment);

            // Add answers
            foreach (var answer in answers)
            {
                var userAnswer = new UserAnswer
                {
                    Id = Guid.NewGuid(),
                    AssessmentId = assessment.Id,
                    QuestionId = answer.Key,
                    OptionId = answer.Value
                };
                _context.UserAnswers.Add(userAnswer);
            }

            await _context.SaveChangesAsync();

            return ServiceResult<Guid>.SuccessResult(assessment.Id, "Assessment completed successfully!");
        }
        catch (Exception ex)
        {
            return ServiceResult<Guid>.FailureResult($"Failed to submit assessment: {ex.Message}");
        }
    }

    public async Task<DTOs.Assessment.AssessmentResult?> GetAssessmentResultAsync(Guid assessmentId)
    {
        try
        {
            var assessment = await _context.UserAssessments
                .Include(a => a.Answers)
                .ThenInclude(a => a.Option)
                .FirstOrDefaultAsync(a => a.Id == assessmentId);

            if (assessment == null)
                return null;

            var selectedOptions = assessment.Answers.Select(a => a.Option).ToList();
            var skillLevels = AnalyzeSkillLevels(selectedOptions);
            var recommendedCategories = AnalyzeRecommendedCategories(selectedOptions);

            return new AssessmentResultDto
            {
                Id = assessment.Id,
                UserId = assessment.UserId,
                CompletedAt = assessment.CompletedAt,
                SkillLevels = skillLevels,
                RecommendedCategories = recommendedCategories,
                Summary = assessment.ResultSummary
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<DTOs.Assessment.CourseRecommendation>> GetRecommendationsAsync(Guid userId)
    {
        try
        {
            // Get latest assessment
            var latestAssessment = await _context.UserAssessments
                .Include(a => a.Answers)
                .ThenInclude(a => a.Option)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CompletedAt)
                .FirstOrDefaultAsync();

            if (latestAssessment == null)
            {
                // Return general recommendations
                return await GetGeneralRecommendationsAsync();
            }

            var selectedOptions = latestAssessment.Answers.Select(a => a.Option).ToList();
            var skillLevels = AnalyzeSkillLevels(selectedOptions);
            var recommendedCategories = AnalyzeRecommendedCategories(selectedOptions);

            // Get the dominant skill level (e.g., Programming) or default
            var userLevel = skillLevels.ContainsKey("Programming") ? skillLevels["Programming"] : "Beginner";

            // Get courses based on recommendations
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => recommendedCategories.Contains(c.Category.Name))
                .Take(12)
                .Select(c => new CourseRecommendationDto
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category.Name,
                    Level = userLevel, 
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    InstructorName = c.Instructor.Username,
                    // Match score based on interest rank
                    MatchScore = 95 - (recommendedCategories.IndexOf(c.Category.Name) * 10),
                    MatchReason = $"This course aligns with your interest in {c.Category.Name} and your {userLevel.ToLower()} skill level."
                })
                .OrderByDescending(c => c.MatchScore)
                .ToListAsync();

            return courses;
        }
        catch
        {
            return await GetGeneralRecommendationsAsync();
        }
    }

    private List<DTOs.Assessment.AssessmentQuestion> GetDefaultQuestions()
    {
        return new List<AssessmentQuestionDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                QuestionText = "What is your current experience level with programming?",
                QuestionType = "SingleChoice",
                OrderIndex = 1,
                Options = new List<AssessmentOptionDto>
                {
                    new() { Id = Guid.NewGuid(), OptionText = "Complete beginner - I've never coded before", SkillLevel = "Beginner", Category = "Programming" },
                    new() { Id = Guid.NewGuid(), OptionText = "Some experience - I've done basic tutorials", SkillLevel = "Intermediate", Category = "Programming" },
                    new() { Id = Guid.NewGuid(), OptionText = "Intermediate - I can build simple applications", SkillLevel = "Intermediate", Category = "Programming" },
                    new() { Id = Guid.NewGuid(), OptionText = "Advanced - I'm comfortable with complex projects", SkillLevel = "Advanced", Category = "Programming" }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                QuestionText = "Which area interests you most?",
                QuestionType = "SingleChoice",
                OrderIndex = 2,
                Options = new List<AssessmentOptionDto>
                {
                    new() { Id = Guid.NewGuid(), OptionText = "Web Development - Building websites and web applications", SkillLevel = "None", Category = "Web Development" },
                    new() { Id = Guid.NewGuid(), OptionText = "Data Science - Analyzing data and machine learning", SkillLevel = "None", Category = "Data Science" },
                    new() { Id = Guid.NewGuid(), OptionText = "Design - UI/UX and graphic design", SkillLevel = "None", Category = "Design" },
                    new() { Id = Guid.NewGuid(), OptionText = "Business - Management and entrepreneurship", SkillLevel = "None", Category = "Business" }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                QuestionText = "What is your primary goal for learning?",
                QuestionType = "SingleChoice",
                OrderIndex = 3,
                Options = new List<AssessmentOptionDto>
                {
                    new() { Id = Guid.NewGuid(), OptionText = "Career change - I want to switch to a new field", SkillLevel = "None", Category = "Career Development" },
                    new() { Id = Guid.NewGuid(), OptionText = "Skill improvement - I want to advance in my current role", SkillLevel = "None", Category = "Professional Growth" },
                    new() { Id = Guid.NewGuid(), OptionText = "Personal interest - Learning for fun and curiosity", SkillLevel = "None", Category = "Personal Development" },
                    new() { Id = Guid.NewGuid(), OptionText = "Academic requirements - For school or certification", SkillLevel = "None", Category = "Academic" }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                QuestionText = "How much time can you dedicate to learning per week?",
                QuestionType = "SingleChoice",
                OrderIndex = 4,
                Options = new List<AssessmentOptionDto>
                {
                    new() { Id = Guid.NewGuid(), OptionText = "1-3 hours - I have limited time but want to learn", SkillLevel = "None", Category = "Time Management" },
                    new() { Id = Guid.NewGuid(), OptionText = "4-7 hours - I can dedicate regular time to learning", SkillLevel = "None", Category = "Time Management" },
                    new() { Id = Guid.NewGuid(), OptionText = "8-15 hours - Learning is a high priority for me", SkillLevel = "None", Category = "Time Management" },
                    new() { Id = Guid.NewGuid(), OptionText = "15+ hours - I'm fully committed to intensive learning", SkillLevel = "None", Category = "Time Management" }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                QuestionText = "Which learning style works best for you?",
                QuestionType = "SingleChoice",
                OrderIndex = 5,
                Options = new List<AssessmentOptionDto>
                {
                    new() { Id = Guid.NewGuid(), OptionText = "Video tutorials - I learn best by watching demonstrations", SkillLevel = "None", Category = "Learning Style" },
                    new() { Id = Guid.NewGuid(), OptionText = "Hands-on projects - I prefer learning by doing", SkillLevel = "None", Category = "Learning Style" },
                    new() { Id = Guid.NewGuid(), OptionText = "Reading and articles - I like detailed written explanations", SkillLevel = "None", Category = "Learning Style" },
                    new() { Id = Guid.NewGuid(), OptionText = "Interactive exercises - I enjoy quizzes and practice problems", SkillLevel = "None", Category = "Learning Style" }
                }
            }
        };
    }

    private Dictionary<string, string> AnalyzeSkillLevels(List<Data.Database.Entities.AssessmentOption> selectedOptions)
    {
        var skillLevels = new Dictionary<string, string>();

        // Group by category and take the most frequent or highest skill level
        var skillGroups = selectedOptions
            .Where(o => !string.IsNullOrEmpty(o.SkillLevel) && o.SkillLevel != "None")
            .GroupBy(o => o.Category ?? "General");

        foreach (var group in skillGroups)
        {
            // Simple logic: Use the most common skill level for that category
            var topLevel = group.GroupBy(x => x.SkillLevel)
                                .OrderByDescending(g => g.Count())
                                .First().Key;
            
            skillLevels[group.Key] = topLevel;
        }

        // If specific categories are missing but we have a general programming level
        if (!skillLevels.ContainsKey("Programming"))
        {
            var progOption = selectedOptions.FirstOrDefault(o => o.Category == "Programming");
            if (progOption != null) skillLevels["Programming"] = progOption.SkillLevel;
        }

        return skillLevels;
    }

    private List<string> AnalyzeRecommendedCategories(List<Data.Database.Entities.AssessmentOption> selectedOptions)
    {
        // Count interest hits per category
        var categoryScores = selectedOptions
            .Where(o => !string.IsNullOrEmpty(o.Category))
            .GroupBy(o => o.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        // Filters out meta-categories
        var metaCategories = new[] { "Programming", "Career Development", "Time Management", "Learning Style", "Academic", "Professional Growth", "Personal Development" };
        
        var recommendations = categoryScores
            .Where(c => !metaCategories.Contains(c.Category))
            .Select(c => c.Category)
            .ToList();

        // Fallback if no specific tech categories were found
        if (!recommendations.Any())
        {
            recommendations.AddRange(new[] { "Web Development", "Data Science", "Design" });
        }

        return recommendations;
    }

    private string GenerateSummary(Dictionary<string, string> skillLevels, List<string> recommendedCategories)
    {
        var summary = "Based on your assessment, we've identified your learning preferences and skill levels. ";
        
        if (skillLevels.Any())
        {
            summary += $"Your current programming level is {skillLevels.FirstOrDefault().Value?.ToLower()}. ";
        }

        if (recommendedCategories.Any())
        {
            summary += $"We recommend focusing on {string.Join(", ", recommendedCategories)} courses to match your interests and goals.";
        }

        return summary;
    }

    private async Task<List<DTOs.Assessment.CourseRecommendation>> GetGeneralRecommendationsAsync()
    {
        try
        {
            // Return popular courses as general recommendations
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Take(8)
                .Select(c => new CourseRecommendationDto
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category.Name,
                    Level = "All Levels",
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    InstructorName = c.Instructor.Username,
                    MatchScore = 75,
                    MatchReason = "Popular course for beginners"
                })
                .ToListAsync();

            return courses;
        }
        catch
        {
            return new List<CourseRecommendationDto>();
        }
    }
}
