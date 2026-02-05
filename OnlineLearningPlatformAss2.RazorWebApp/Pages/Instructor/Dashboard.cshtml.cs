using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Instructor;

[Authorize(Roles = "Instructor")]
public class DashboardModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly INotificationService _notificationService;

    public DashboardModel(ICourseService courseService, INotificationService notificationService)
    {
        _courseService = courseService;
        _notificationService = notificationService;
    }

    public List<CourseViewModel> MyCourses { get; set; } = new();
    public int TotalStudents { get; set; }
    public decimal TotalEarnings { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        var courses = await _courseService.GetInstructorCoursesAsync(userId);
        MyCourses = courses.ToList();
        
        TotalStudents = MyCourses.Sum(c => c.StudentCount);
        TotalEarnings = await _courseService.GetInstructorEarningsAsync(userId);
        AverageRating = MyCourses.Any(c => c.Rating > 0) ? MyCourses.Where(c => c.Rating > 0).Average(c => c.Rating) : 0;
        
        Notifications = (await _notificationService.GetUserNotificationsAsync(userId)).Take(5).ToList();
        UnreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

        return Page();
    }

    public decimal AverageRating { get; set; }
    public List<OnlineLearningPlatformAss2.Data.Database.Entities.Notification> Notifications { get; set; } = new();
    public int UnreadNotifications { get; set; }

    public async Task<IActionResult> OnPostMarkAsReadAsync(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return new JsonResult(new { success = true });
    }
}
