namespace OnlineLearningPlatformAss2.Service.DTOs.Category;

public class CategoryViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CourseCount { get; set; }
    
    // Helper properties
    public string DisplayText => $"{Name} ({CourseCount})";
}
