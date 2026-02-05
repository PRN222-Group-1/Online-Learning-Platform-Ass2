namespace OnlineLearningPlatformAss2.Service.DTOs.Course;

public class CourseListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int StudentCount { get; set; }
    public int TotalLessons { get; set; }
    public int Duration { get; set; } // in minutes
    public string Level { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsBestseller { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Helper properties
    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
    public string FormattedDuration => Duration > 0 ? $"{Duration / 60}h {Duration % 60}m" : "TBD";
    public string RatingDisplay => new string('★', (int)Math.Round(Rating)) + new string('☆', 5 - (int)Math.Round(Rating));
}

public class CourseDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string? InstructorBio { get; set; }
    public string? InstructorImageUrl { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int StudentCount { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Language { get; set; } = "English";
    public List<string> WhatYouWillLearn { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<CourseModuleDto> Modules { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsEnrolled { get; set; }
    
    // Certificate info
    public bool HasCertificate { get; set; } = true;
    
    // Helper properties
    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
    public int TotalLessons => Modules.Sum(m => m.Lessons.Count);
    public int TotalDuration => Modules.Sum(m => m.Lessons.Sum(l => l.Duration));
    public string FormattedDuration => TotalDuration > 0 ? $"{TotalDuration / 60}h {TotalDuration % 60}m" : "TBD";
    public string RatingDisplay => new string('★', (int)Math.Round(Rating)) + new string('☆', 5 - (int)Math.Round(Rating));
}

public class CourseModuleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public List<CourseLessonDto> Lessons { get; set; } = new();
    
    public int TotalDuration => Lessons.Sum(l => l.Duration);
    public string FormattedDuration => TotalDuration > 0 ? $"{TotalDuration / 60}h {TotalDuration % 60}m" : "TBD";
}

public class CourseLessonDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Duration { get; set; } // in minutes
    public string? VideoUrl { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPreview { get; set; }
    
    public string FormattedDuration => Duration > 0 ? $"{Duration}m" : "TBD";
    public string LessonType => !string.IsNullOrEmpty(VideoUrl) ? "Video" : "Text";
}

public class CourseEnrollmentDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Active"; // Active, Completed, Dropped
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public decimal ProgressPercentage => TotalLessons > 0 ? (decimal)CompletedLessons / TotalLessons * 100 : 0;
}
