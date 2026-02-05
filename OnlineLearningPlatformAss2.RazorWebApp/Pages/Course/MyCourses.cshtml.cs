namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

[Authorize]
public class MyCoursesModel : PageModel
{
    private readonly ICourseService _courseService;

    public MyCoursesModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public IEnumerable<CourseViewModel> EnrolledCourses { get; set; } = new List<CourseViewModel>();

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        EnrolledCourses = await _courseService.GetEnrolledCoursesAsync(userId);
        return Page();
    }
}
