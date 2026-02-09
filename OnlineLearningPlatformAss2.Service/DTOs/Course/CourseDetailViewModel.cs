namespace OnlineLearningPlatformAss2.Service.DTOs.Course;

public class CourseDetailViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = null!;
    public string InstructorName { get; set; } = null!;
    public bool IsEnrolled { get; set; }
    public bool IsInWishlist { get; set; }
    public decimal Rating { get; set; } = 0m;
    public bool IsFeatured { get; set; }
    public int ReviewCount { get; set; }
    public int StudentCount { get; set; }
    public string Level { get; set; } = "All Levels";
    public string Language { get; set; } = "English";
    public List<string> WhatYouWillLearn { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public bool HasCertificate { get; set; } = true;
    public List<ModuleViewModel> Modules { get; set; } = new(); // Changed to List for indexing
    public List<ReviewViewModel> Reviews { get; set; } = new();
    
    // Helper properties
    public string FormattedPrice => Price == 0 ? "Miễn phí" : $"{Price:N0}₫";
    public string RatingDisplay => new string('★', (int)Math.Round(Rating)) + new string('☆', 5 - (int)Math.Round(Rating));
    public int TotalLessons => Modules.Sum(m => m.Lessons.Count());
    public string FormattedDuration => TotalLessons > 0 ? $"{TotalLessons * 15}m" : "TBD"; // Estimate 15min per lesson
}

public class ModuleViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OrderIndex { get; set; }
    public List<LessonViewModel> Lessons { get; set; } = new(); // Changed to List for indexing
    
    // Helper properties
    public string FormattedDuration => Lessons.Sum(l => l.Duration) > 0 ? $"{Lessons.Sum(l => l.Duration)}m" : "TBD";
}

public class LessonViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsCurrent { get; set; }
    public int Duration { get; set; } = 15; // Default 15 minutes
    public bool IsPreview { get; set; }
    public bool IsCompleted { get; set; }
    public int? LastWatchedPosition { get; set; }
    public string? AiSummary { get; set; }
    public string? Transcript { get; set; }
    
    // Helper properties
    public string FormattedDuration => Duration > 0 ? $"{Duration}m" : "TBD";
    public string LessonType => !string.IsNullOrEmpty(VideoUrl) ? "Video" : "Text";
}

public class CourseLearnViewModel
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public LessonViewModel? CurrentLesson { get; set; }
    public Guid? CurrentLessonId { get; set; }
    public decimal Progress { get; set; }
    public Guid? CertificateId { get; set; }
    public List<ModuleViewModel> Modules { get; set; } = new(); // Changed to List for indexing
}
