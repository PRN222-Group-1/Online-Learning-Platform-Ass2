using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

public class DetailsModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly OnlineLearningContext _context;

    public DetailsModel(ICourseService courseService, OnlineLearningContext context)
    {
        _courseService = courseService;
        _context = context;
    }

    public CourseDetailViewModel? Course { get; set; }
    public List<CourseViewModel> RelatedCourses { get; set; } = new();
    public bool IsAuthenticated { get; set; }
    public bool IsEnrolled { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        IsAuthenticated = User.Identity?.IsAuthenticated == true;
        
        Guid? userId = null;
        if (IsAuthenticated && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId))
        {
            userId = parsedUserId;
        }

        Course = await _courseService.GetCourseDetailsAsync(id, userId);
        
        if (Course == null)
        {
            return NotFound();
        }

        IsEnrolled = Course.IsEnrolled;

        // Get related courses from same category
        var allCourses = await _courseService.GetAllCoursesAsync();
        RelatedCourses = allCourses
            .Where(c => c.CategoryName == Course.CategoryName && c.Id != Course.Id)
            .Take(3)
            .ToList();

        return Page();
    }

    [BindProperty]
    public SubmitReviewDto ReviewForm { get; set; } = new();

    public async Task<IActionResult> OnPostSubmitReviewAsync()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/User/Login");
        }

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        if (ReviewForm.Rating < 1 || ReviewForm.Rating > 5)
        {
            return Page();
        }

        await _courseService.SubmitReviewAsync(userId, ReviewForm);
        
        return RedirectToPage(new { id = ReviewForm.CourseId });
    }

    public async Task<IActionResult> OnPostToggleWishlistAsync([FromBody] WishlistRequest request)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return new JsonResult(new { success = false, message = "Please login first" });
        }

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return new JsonResult(new { success = false, message = "Invalid user" });
        }

        var isAdded = await _courseService.ToggleWishlistAsync(userId, request.CourseId);
        
        return new JsonResult(new { success = true, isAdded = isAdded, message = isAdded ? "Added to wishlist" : "Removed from wishlist" });
    }

    public class WishlistRequest
    {
        public Guid CourseId { get; set; }
    }

    public async Task<IActionResult> OnPostEnrollAsync([FromBody] EnrollRequest request)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return new JsonResult(new { success = false, message = "Please login first", requiresLogin = true });
        }

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return new JsonResult(new { success = false, message = "Invalid user", requiresLogin = true });
        }

        try
        {
            var success = await _courseService.EnrollUserAsync(userId, request.CourseId);

            if (!success)
            {
                var course = await _courseService.GetCourseDetailsAsync(request.CourseId);
                if (course != null && course.InstructorName == User.Identity?.Name)
                {
                    return new JsonResult(new { success = false, message = "You cannot enroll in your own course." });
                }
                return new JsonResult(new { success = false, message = "Enrollment failed. You might already be enrolled." });
            }

            return new JsonResult(new { 
                success = true, 
                message = "Successfully enrolled in the course!",
                learnUrl = $"/Course/Learn/{request.CourseId}"
            });
        }
        catch (Exception)
        {
            return new JsonResult(new { success = false, message = "Failed to enroll. Please try again." });
        }
    }

    public class EnrollRequest
    {
        public Guid CourseId { get; set; }
    }
}
