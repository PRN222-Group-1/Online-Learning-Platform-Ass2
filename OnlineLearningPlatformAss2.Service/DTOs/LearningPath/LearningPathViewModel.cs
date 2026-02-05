using OnlineLearningPlatformAss2.Service.DTOs.Course;

namespace OnlineLearningPlatformAss2.Service.DTOs.LearningPath;

public class LearningPathViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    public bool IsCustomPath { get; set; }
    public int CourseCount { get; set; }
    public bool IsEnrolled { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Level { get; set; } = "All Levels";
    public IEnumerable<CourseInPathViewModel> Courses { get; set; } = new List<CourseInPathViewModel>();
    
    // Helper properties
    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
    public string FormattedDuration => CourseCount > 0 ? $"{CourseCount * 3}h" : "TBD"; // Estimate 3h per course
}

public class CourseInPathViewModel
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int OrderIndex { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class UserLearningPathWithProgressDto
{
    public Guid Id { get; set; }
    public Guid PathId { get; set; }
    public string PathTitle { get; set; } = string.Empty;
    public string PathDescription { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Active";
    public int TotalCourses { get; set; }
    public int CompletedCourses { get; set; }
    public decimal Progress { get; set; }
    public string? CurrentCourseTitle { get; set; }
    
    // Helper property
    public bool IsCompleted => CompletedAt.HasValue;
    
    // Legacy properties for compatibility
    public Guid EnrollmentId => Id;
    public string Title => PathTitle;
    public string Description => PathDescription;
    public decimal Price { get; set; }
    public string EnrollmentStatus => Status;
    public int ProgressPercentage => (int)Progress;
    public bool IsCustomPath { get; set; }
}

public class LearningPathDetailsWithProgressDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    public bool IsCustomPath { get; set; }
    public bool IsEnrolled { get; set; }
    public decimal Progress { get; set; }
    public List<string> WhatYouWillLearn { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public List<PathCourseWithProgressDto> Courses { get; set; } = new();
    
    // Legacy properties for compatibility
    public int ProgressPercentage => (int)Progress;
}

public class PathCourseWithProgressDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int OrderIndex { get; set; }
    public int Duration { get; set; } // in minutes
    public string Level { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    
    // Progress info (if enrolled)
    public bool IsCompleted { get; set; }
    public bool IsCurrentCourse { get; set; }
    public bool IsLocked { get; set; }
    public decimal Progress { get; set; }
    
    // Helper properties
    public string FormattedDuration => Duration > 0 ? $"{Duration / 60}h {Duration % 60}m" : "TBD";
    public string StatusText => IsCompleted ? "Completed" : IsCurrentCourse ? "In Progress" : IsLocked ? "Locked" : "Available";
    public string StatusClass => IsCompleted ? "completed" : IsCurrentCourse ? "current" : IsLocked ? "locked" : "available";
}
