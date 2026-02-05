namespace OnlineLearningPlatformAss2.RazorWebApp.Models;

public class HomeViewModel
{
    public IEnumerable<CourseViewModel> FeaturedCourses { get; set; } = new List<CourseViewModel>();
    public IEnumerable<LearningPathViewModel> FeaturedPaths { get; set; } = new List<LearningPathViewModel>();
    
    // Search & Filter
    public string? SearchTerm { get; set; }
    public Guid? SelectedCategoryId { get; set; }
    public bool ViewAll { get; set; } = false;
    public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
}
