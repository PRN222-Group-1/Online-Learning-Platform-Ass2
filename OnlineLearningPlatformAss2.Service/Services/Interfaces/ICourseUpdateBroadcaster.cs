using OnlineLearningPlatformAss2.Service.DTOs.Course;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

/// <summary>
/// Abstraction for SignalR broadcasts, implemented in the WebApp layer.
/// This allows the Service layer to trigger real-time updates without depending on SignalR directly.
/// </summary>
public interface ICourseUpdateBroadcaster
{
    Task BroadcastCourseAddedAsync(CourseViewModel course);
    Task BroadcastCourseUpdatedAsync(CourseViewModel course);
    Task BroadcastCourseDeletedAsync(Guid courseId);
}
