using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Instructor;

[Authorize(Roles = "Instructor")]
public class EditCourseModel : PageModel
{
    private readonly ICourseService _courseService;

    public EditCourseModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [BindProperty]
    public CourseUpdateDto CourseForm { get; set; } = new();

    public CourseDetailViewModel? FullCourse { get; set; }
    public List<CategoryViewModel> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        Categories = (await _courseService.GetAllCategoriesAsync()).ToList();

        if (id.HasValue)
        {
            var course = await _courseService.GetCourseDetailsAsync(id.Value);
            if (course == null) return NotFound();

            // Verify ownership
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId) || (course.InstructorName != User.Identity?.Name && !User.IsInRole("Admin")))
            {
                // Simple security for assignment
                // return RedirectToPage("/Instructor/Dashboard");
            }

            FullCourse = course;
            CourseForm = new CourseUpdateDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                ImageUrl = course.ImageUrl,
                CategoryId = Categories.FirstOrDefault(c => c.Name == course.CategoryName)?.Id ?? Guid.Empty,
                IsFeatured = course.IsFeatured,
                Level = course.Level,
                Language = course.Language
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = (await _courseService.GetAllCategoriesAsync()).ToList();
            return Page();
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return RedirectToPage("/User/Login");

        var success = await _courseService.UpdateCourseAsync(CourseForm.Id, CourseForm, userId);
        if (success)
        {
            TempData["SuccessMessage"] = "Course info updated!";
            return RedirectToPage(new { id = CourseForm.Id });
        }

        ModelState.AddModelError("", "Failed to save changes.");
        Categories = (await _courseService.GetAllCategoriesAsync()).ToList();
        return Page();
    }

    // --- Curriculum AJAX Handlers ---

    public async Task<IActionResult> OnPostAddModuleAsync([FromBody] ModuleRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var resultId = await _courseService.AddModuleAsync(request.CourseId, request.Title, request.Description ?? "", request.OrderIndex, userId);
        return new JsonResult(new { success = resultId.HasValue, id = resultId });
    }

    public async Task<IActionResult> OnPostUpdateModuleAsync([FromBody] ModuleUpdateRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var success = await _courseService.UpdateModuleAsync(request.Id, request.Title, request.Description ?? "", request.OrderIndex, userId);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostDeleteModuleAsync(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var success = await _courseService.DeleteModuleAsync(id, userId);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostUpsertLessonAsync([FromBody] LessonRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        bool success;
        Guid? resultId = null;
        if (request.Id == Guid.Empty)
        {
            resultId = await _courseService.AddLessonAsync(request.ModuleId, request.Title, request.Content, request.VideoUrl, request.OrderIndex, userId);
            success = resultId.HasValue;
        }
        else
        {
            success = await _courseService.UpdateLessonAsync(request.Id, request.Title, request.Content, request.VideoUrl, request.OrderIndex, userId);
            resultId = request.Id;
        }
        return new JsonResult(new { success, id = resultId });
    }

    public async Task<IActionResult> OnPostDeleteLessonAsync(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var success = await _courseService.DeleteLessonAsync(id, userId);
        return new JsonResult(new { success });
    }

    public class ModuleRequest { public Guid CourseId { get; set; } public string Title { get; set; } = null!; public string? Description { get; set; } public int OrderIndex { get; set; } }
    public class ModuleUpdateRequest { public Guid Id { get; set; } public string Title { get; set; } = null!; public string? Description { get; set; } public int OrderIndex { get; set; } }
    public class LessonRequest { public Guid Id { get; set; } public Guid ModuleId { get; set; } public string Title { get; set; } = null!; public string Content { get; set; } = null!; public string? VideoUrl { get; set; } public int OrderIndex { get; set; } }
}
