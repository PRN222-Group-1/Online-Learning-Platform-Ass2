using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CourseManagementModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly IAdminService _adminService;

    public CourseManagementModel(ICourseService courseService, IAdminService adminService)
    {
        _courseService = courseService;
        _adminService = adminService;
    }

    public IEnumerable<CourseViewModel> Courses { get; set; } = new List<CourseViewModel>();

    public async Task OnGetAsync()
    {
        Courses = await _adminService.GetAllCoursesAsync();
    }

    public async Task<IActionResult> OnPostSuspendAsync(Guid id)
    {
        await _adminService.SuspendCourseAsync(id);
        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostUnsuspendAsync(Guid id)
    {
        await _adminService.UnsuspendCourseAsync(id);
        return new JsonResult(new { success = true });
    }
}
