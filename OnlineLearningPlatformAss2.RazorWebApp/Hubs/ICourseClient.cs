using OnlineLearningPlatformAss2.Service.DTOs.Course;

namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

public interface ICourseClient
{
    Task ReceiveCourseAdded(CourseViewModel course);
    Task ReceiveCourseUpdated(CourseViewModel course);
    Task ReceiveCourseDeleted(Guid courseId);
}
