using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ILearningPathService _learningPathService;
    private readonly IUserService _userService;

    public IndexModel(ICourseService courseService, ILearningPathService learningPathService, IUserService userService)
    {
        _courseService = courseService;
        _learningPathService = learningPathService;
        _userService = userService;
    }

    public List<CourseViewModel> Courses { get; set; } = new();
    public List<CourseViewModel> EnrolledCourses { get; set; } = new();
    public List<LearningPathViewModel> FeaturedPaths { get; set; } = new();
    public List<CategoryViewModel> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public bool ViewAll { get; set; }
    public string? SearchTerm { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool HasCompletedAssessment { get; set; }

    public async Task OnGetAsync(string? category = null, bool viewAll = false, string? searchTerm = null)
    {
        IsAuthenticated = User.Identity?.IsAuthenticated == true;
        SelectedCategory = category;
        ViewAll = viewAll;
        SearchTerm = searchTerm;

        var userIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        Guid? userId = Guid.TryParse(userIdString, out var parsedId) ? parsedId : null;

        if (userId.HasValue)
        {
            HasCompletedAssessment = await _userService.HasCompletedAssessmentAsync(userId.Value);
        }

        // Load categories from database
        var categoriesFromDb = await _courseService.GetAllCategoriesAsync();
        Categories = categoriesFromDb.ToList();
        
        // Get courses from database
        await LoadCoursesAsync(userId);
        
        // Get featured learning paths from database
        await LoadLearningPathsAsync();

        // Get enrolled courses if authenticated
        if (IsAuthenticated && userId.HasValue)
        {
            var enrolled = await _courseService.GetEnrolledCoursesAsync(userId.Value);
            EnrolledCourses = enrolled.Take(4).ToList();
        }
    }

    private async Task LoadCoursesAsync(Guid? userId)
    {
        // Get courses from database
        IEnumerable<CourseViewModel> allCourses;
        
        if (ViewAll || !string.IsNullOrEmpty(SearchTerm))
        {
            allCourses = await _courseService.GetAllCoursesAsync(SearchTerm, userId);
        }
        else
        {
            allCourses = await _courseService.GetFeaturedCoursesAsync();
            
            // If authenticated, we need to manually update IsEnrolled for featured courses 
            // since GetFeaturedCoursesAsync might not be checking it
            if (userId.HasValue)
            {
                var enrolledIds = (await _courseService.GetEnrolledCoursesAsync(userId.Value)).Select(c => c.Id).ToHashSet();
                foreach (var course in allCourses)
                {
                    course.IsEnrolled = enrolledIds.Contains(course.Id);
                }
            }
        }

        Courses = allCourses.ToList();

        // Filter by category if specified
        if (!string.IsNullOrEmpty(SelectedCategory))
        {
            Courses = Courses.Where(c => c.CategoryName.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Limit courses if not viewing all and no search term
        if (!ViewAll && string.IsNullOrEmpty(SearchTerm))
        {
            Courses = Courses.Take(6).ToList();
        }
    }

    private async Task LoadLearningPathsAsync()
    {
        var paths = await _learningPathService.GetFeaturedLearningPathsAsync();
        FeaturedPaths = paths.ToList();
    }
}
