using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.DTOs.Review;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class ReviewService : IReviewService
{
    private readonly OnlineLearningContext _context;

    public ReviewService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<bool> AddReviewAsync(Guid userId, ReviewRequest request)
    {
        // Check if user already reviewed
        bool exists = await _context.CourseReviews.AnyAsync(r => r.UserId == userId && r.CourseId == request.CourseId);
        if (exists) return false;

        // Verify enrollment and completion (best practice)
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId && e.Status == "Completed");
        
        if (enrollment == null) return false;

        // Prevent instructor from reviewing their own course
        if (enrollment.Course.InstructorId == userId) return false;

        var review = new CourseReview
        {
            Id = Guid.NewGuid(),
            CourseId = request.CourseId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.CourseReviews.Add(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ReviewViewModel>> GetCourseReviewsAsync(Guid courseId)
    {
        return await _context.CourseReviews
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewViewModel
            {
                Id = r.Id,
                Username = r.User.Username,
                AvatarUrl = r.User.Profile.AvatarUrl,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CourseRatingSummary> GetRatingSummaryAsync(Guid courseId)
    {
        var reviews = await _context.CourseReviews
            .Where(r => r.CourseId == courseId)
            .Select(r => r.Rating)
            .ToListAsync();

        if (!reviews.Any())
        {
            return new CourseRatingSummary { AverageRating = 0, TotalReviews = 0 };
        }

        return new CourseRatingSummary
        {
            AverageRating = Math.Round(reviews.Average(), 1),
            TotalReviews = reviews.Count
        };
    }

    public async Task<bool> HasUserReviewedAsync(Guid userId, Guid courseId)
    {
        return await _context.CourseReviews.AnyAsync(r => r.UserId == userId && r.CourseId == courseId);
    }
}
