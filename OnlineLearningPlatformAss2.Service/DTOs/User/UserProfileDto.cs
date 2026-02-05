using OnlineLearningPlatformAss2.Service.DTOs.Course;

namespace OnlineLearningPlatformAss2.Service.DTOs.User;

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    string? Role,
    DateTime CreatedAt,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Address,
    DateTime? DateOfBirth,
    string? AvatarUrl,
    int EnrolledCoursesCount,
    int CompletedCoursesCount,
    decimal OverallProgress,
    bool HasCompletedAssessment,
    IEnumerable<CourseViewModel> RecentCourses
);
