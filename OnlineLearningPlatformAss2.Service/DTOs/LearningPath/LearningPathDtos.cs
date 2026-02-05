namespace OnlineLearningPlatformAss2.Service.DTOs.LearningPath;

public class LearningPathListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsCustomPath { get; set; }
    public int CourseCount { get; set; }
    public int TotalDuration { get; set; } // in minutes
    public string Level { get; set; } = "All Levels";
    public DateTime CreatedAt { get; set; }
    
    // User enrollment info (if applicable)
    public bool IsEnrolled { get; set; }
    public decimal Progress { get; set; }
    public DateTime? EnrolledAt { get; set; }
    
    // Helper properties
    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
    public string FormattedDuration => TotalDuration > 0 ? $"{TotalDuration / 60}h {TotalDuration % 60}m" : "TBD";
    public string BadgeText => IsCustomPath ? "Personalized" : "Popular";
}

public class LearningPathDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsCustomPath { get; set; }
    public string Level { get; set; } = "All Levels";
    public List<string> WhatYouWillLearn { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public List<LearningPathCourseDto> Courses { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    
    // User enrollment info
    public bool IsEnrolled { get; set; }
    public decimal Progress { get; set; }
    public DateTime? EnrolledAt { get; set; }
    public Guid? CurrentCourseId { get; set; }
    
    // Helper properties
    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
    public int TotalCourses => Courses.Count;
    public int TotalDuration => Courses.Sum(c => c.Duration);
    public string FormattedDuration => TotalDuration > 0 ? $"{TotalDuration / 60}h {TotalDuration % 60}m" : "TBD";
    public int CompletedCourses => Courses.Count(c => c.IsCompleted);
}

public class LearningPathCourseDto
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

public class UserLearningPathEnrollmentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PathId { get; set; }
    public string PathTitle { get; set; } = string.Empty;
    public string PathDescription { get; set; } = string.Empty;
    public decimal PathPrice { get; set; }
    public string? PathImageUrl { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Active";
    
    // Progress info
    public int TotalCourses { get; set; }
    public int CompletedCourses { get; set; }
    public decimal Progress { get; set; }
    public Guid? CurrentCourseId { get; set; }
    public string? CurrentCourseTitle { get; set; }
    
    // Helper properties
    public string FormattedPrice => PathPrice == 0 ? "Free" : PathPrice.ToString("C");
    public bool IsCompleted => CompletedAt.HasValue;
    public string ProgressText => $"{CompletedCourses} of {TotalCourses} courses completed";
}

public class PersonalizedLearningPathDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> CourseIds { get; set; } = new();
    public List<string> RecommendationReasons { get; set; } = new();
    public string SkillLevel { get; set; } = string.Empty;
    public List<string> InterestCategories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    
    // Generated from assessment
    public Guid AssessmentId { get; set; }
    public Dictionary<string, string> AssessmentResults { get; set; } = new();
}

public class LearningPathRecommendationDto
{
    public Guid PathId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CourseCount { get; set; }
    public string Level { get; set; } = string.Empty;
    public int MatchScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public List<string> KeySkills { get; set; } = new();
    public bool IsPersonalized { get; set; }
    
    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
}
