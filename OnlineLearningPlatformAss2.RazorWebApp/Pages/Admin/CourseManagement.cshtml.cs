using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    public IEnumerable<AdminUserDto> Instructors { get; set; } = new List<AdminUserDto>();

    public async Task OnGetAsync()
    {
        Courses = await _adminService.GetAllCoursesAsync();
        Categories = await _courseService.GetAllCategoriesAsync();
        Instructors = await _adminService.GetAllInstructorsAsync();
    }

    public async Task<IActionResult> OnPostCreateCourseAsync([FromBody] CourseCreateDto dto)
    {
        var success = await _adminService.CreateCourseAsync(dto);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostUpdateCourseAsync(Guid id, [FromBody] CourseUpdateDto dto)
    {
        var success = await _adminService.UpdateCourseAsync(id, dto);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostDeleteCourseAsync(Guid id)
    {
        var success = await _adminService.DeleteCourseAsync(id);
        return new JsonResult(new { success });
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

    public async Task<IActionResult> OnGetCurriculumAsync(Guid courseId)
    {
        var curriculum = await _adminService.GetCourseCurriculumAsync(courseId);
        return new JsonResult(curriculum);
    }

    public async Task<IActionResult> OnPostAddModuleAsync(Guid courseId, string title, string? description, int orderIndex)
    {
        var success = await _adminService.AddModuleAsync(courseId, title, description ?? "", orderIndex);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostUpdateModuleAsync(Guid moduleId, string title, string? description, int orderIndex)
    {
        var success = await _adminService.UpdateModuleAsync(moduleId, title, description ?? "", orderIndex);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostDeleteModuleAsync(Guid moduleId)
    {
        var success = await _adminService.DeleteModuleAsync(moduleId);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostAddLessonAsync(Guid moduleId, string title, string? content, string? videoUrl, int duration, int orderIndex)
    {
        var success = await _adminService.AddLessonAsync(moduleId, title, content ?? "", videoUrl, duration, orderIndex);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostUpdateLessonAsync(Guid lessonId, string title, string? content, string? videoUrl, int duration, int orderIndex)
    {
        var success = await _adminService.UpdateLessonAsync(lessonId, title, content ?? "", videoUrl, duration, orderIndex);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostDeleteLessonAsync(Guid lessonId)
    {
        var success = await _adminService.DeleteLessonAsync(lessonId);
        return new JsonResult(new { success });
    }
}
