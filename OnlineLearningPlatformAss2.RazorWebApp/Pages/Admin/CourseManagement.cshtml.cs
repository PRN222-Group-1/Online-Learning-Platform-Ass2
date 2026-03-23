using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
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
    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    public IEnumerable<InstructorDto> Instructors { get; set; } = new List<InstructorDto>();

    public async Task OnGetAsync()
    {
        Courses = await _adminService.GetAllCoursesAsync();
        var formData = await _adminService.GetFormDataAsync();
        Categories = formData.Categories;
        Instructors = formData.Instructors;
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

    public async Task<IActionResult> OnPostCreateAsync([FromBody] AdminCreateCourseDto dto)
    {
        var result = await _adminService.CreateCourseAsync(dto);
        return new JsonResult(new { success = result != null, course = result });
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromBody] AdminUpdateCourseDto dto)
    {
        var result = await _adminService.UpdateCourseAsync(dto);
        return new JsonResult(new { success = result != null, course = result });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var success = await _adminService.DeleteCourseAsync(id);
        return new JsonResult(new { success });
    }
}
