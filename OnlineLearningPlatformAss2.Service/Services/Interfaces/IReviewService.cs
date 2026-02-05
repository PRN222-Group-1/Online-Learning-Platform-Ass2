using OnlineLearningPlatformAss2.Service.DTOs.Review;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IReviewService
{
    Task<bool> AddReviewAsync(Guid userId, ReviewRequest request);
    Task<List<ReviewViewModel>> GetCourseReviewsAsync(Guid courseId);
    Task<CourseRatingSummary> GetRatingSummaryAsync(Guid courseId);
    Task<bool> HasUserReviewedAsync(Guid userId, Guid courseId);
}
