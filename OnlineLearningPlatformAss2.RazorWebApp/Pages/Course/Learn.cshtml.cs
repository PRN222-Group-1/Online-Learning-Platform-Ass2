using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Quiz;
using OnlineLearningPlatformAss2.Service.DTOs.Discussion;
using OnlineLearningPlatformAss2.Service.DTOs.Review;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

[Authorize]
public class LearnModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly IQuizService _quizService;
    private readonly IDiscussionService _discussionService;
    private readonly IReviewService _reviewService;

    public LearnModel(ICourseService courseService, IQuizService quizService, IDiscussionService discussionService, IReviewService reviewService)
    {
        _courseService = courseService;
        _quizService = quizService;
        _discussionService = discussionService;
        _reviewService = reviewService;
    }

    public CourseLearnViewModel? CourseLearn { get; set; }
    public bool HasReviewed { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        try
        {
            var enrollmentId = await _courseService.GetEnrollmentIdAsync(userId, id);

            if (enrollmentId.HasValue)
            {
                CourseLearn = await _courseService.GetCourseLearnAsync(enrollmentId.Value);
                HasReviewed = await _reviewService.HasUserReviewedAsync(userId, enrollmentId.Value); // Check by enrollment or course? Actually Review is by Course.
                // Re-check ReviewService: it uses courseId.
                HasReviewed = await _reviewService.HasUserReviewedAsync(userId, id);
            }
            
            if (CourseLearn == null)
            {
                ErrorMessage = "You are not enrolled in this course. Please purchase the course or learning path to access this content.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading course: {ex.Message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCompleteLessonAsync([FromBody] CompleteLessonRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return new JsonResult(new { success = false, message = "User not found" });
        }

        var enrollmentId = await _courseService.GetEnrollmentIdAsync(userId, request.CourseId);
        if (!enrollmentId.HasValue)
        {
            return new JsonResult(new { success = false, message = "Enrollment not found" });
        }

        // Check if lesson has a quiz and if it's passed
        var quiz = await _quizService.GetQuizForLessonAsync(request.LessonId);
        if (quiz != null)
        {
            var passed = await _quizService.HasPassedQuizAsync(userId, quiz.Id);
            if (!passed)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = "Bạn cần vượt qua bài kiểm tra (Quiz) của bài học này (đạt ít nhất 80%) trước khi đánh dấu hoàn thành." 
                });
            }
        }

        var success = await _courseService.UpdateLessonProgressAsync(enrollmentId.Value, request.LessonId, true);
        
        // Fetch updated progress
        var updatedLearn = await _courseService.GetCourseLearnAsync(enrollmentId.Value);

        return new JsonResult(new { 
            success, 
            progress = updatedLearn?.Progress ?? 0,
            isCourseCompleted = updatedLearn?.Progress >= 100
        });
    }

    public async Task<IActionResult> OnGetQuizAsync(Guid lessonId)
    {
        var quiz = await _quizService.GetQuizForLessonAsync(lessonId);
        if (quiz == null) return NotFound();
        return new JsonResult(quiz);
    }

    public async Task<IActionResult> OnPostSubmitQuizAsync([FromBody] QuizSubmissionDto submission)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var result = await _quizService.SubmitAttemptAsync(userId, submission);
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnGetCommentsAsync(Guid lessonId)
    {
        var comments = await _discussionService.GetLessonCommentsAsync(lessonId);
        return new JsonResult(comments);
    }

    public async Task<IActionResult> OnPostPostCommentAsync([FromBody] CommentRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var comment = await _discussionService.PostCommentAsync(userId, request);
        return new JsonResult(comment);
    }

    public async Task<IActionResult> OnPostSubmitReviewAsync([FromBody] ReviewRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var result = await _reviewService.AddReviewAsync(userId, request);
        return new JsonResult(new { success = result });
    }

    public class CompleteLessonRequest
    {
        public Guid CourseId { get; set; }
        public Guid LessonId { get; set; }
    }

    private CourseLearnViewModel CreateSampleLearnSession(Guid courseId)
    {
        return new CourseLearnViewModel
        {
            EnrollmentId = Guid.NewGuid(),
            CourseId = courseId,
            CourseTitle = "Complete Web Development Bootcamp",
            Progress = 25,
            CurrentLessonId = Guid.NewGuid(),
            CurrentLesson = new LessonViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Setting Up Your Development Environment",
                Content = "In this lesson, you'll learn how to set up your development environment with all the necessary tools for web development.",
                VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", // Demo video
                Duration = 15,
                OrderIndex = 2
            },
            Modules = new List<ModuleViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Introduction to Web Development",
                    Description = "Getting started with web development fundamentals",
                    OrderIndex = 1,
                    Lessons = new List<LessonViewModel>
                    {
                        new() { Id = Guid.NewGuid(), Title = "Welcome to the Course", Duration = 5, OrderIndex = 1, IsCompleted = true },
                        new() { Id = Guid.NewGuid(), Title = "Setting Up Your Development Environment", Duration = 15, OrderIndex = 2, IsCurrent = true },
                        new() { Id = Guid.NewGuid(), Title = "How the Internet Works", Duration = 20, OrderIndex = 3 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "HTML Fundamentals",
                    Description = "Master HTML5 and semantic markup",
                    OrderIndex = 2,
                    Lessons = new List<LessonViewModel>
                    {
                        new() { Id = Guid.NewGuid(), Title = "HTML Structure and Elements", Duration = 25, OrderIndex = 1 },
                        new() { Id = Guid.NewGuid(), Title = "Forms and Input Elements", Duration = 30, OrderIndex = 2 },
                        new() { Id = Guid.NewGuid(), Title = "Semantic HTML5 Elements", Duration = 20, OrderIndex = 3 }
                    }
                }
            }
        };
    }
}
