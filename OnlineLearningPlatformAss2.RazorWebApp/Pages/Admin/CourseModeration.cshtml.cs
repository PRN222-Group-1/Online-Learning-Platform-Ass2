using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CourseModerationModel : PageModel
{
    private readonly IAdminService _adminService;

    public CourseModerationModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public IEnumerable<CourseViewModel> PendingCourses { get; set; } = new List<CourseViewModel>();

    public async Task OnGetAsync()
    {
        PendingCourses = await _adminService.GetPendingCoursesAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var success = await _adminService.ApproveCourseAsync(id);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, string reason)
    {
        var success = await _adminService.RejectCourseAsync(id, reason);
        return new JsonResult(new { success });
    }
}
