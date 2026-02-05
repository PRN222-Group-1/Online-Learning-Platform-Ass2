using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using OnlineLearningPlatformAss2.Service.DTOs.Review;
using FluentAssertions;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class ReviewServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    #region AddReviewAsync Tests

    [Fact]
    public async Task AddReviewAsync_ValidReview_ShouldSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);
        var userId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", InstructorId = instructorId });
        context.Enrollments.Add(new Enrollment { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, Status = "Completed" });
        await context.SaveChangesAsync();

        var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Great course!" };

        // Act
        var result = await service.AddReviewAsync(userId, request);

        // Assert
        result.Should().BeTrue();
        var review = await context.CourseReviews.FirstOrDefaultAsync();
        review.Should().NotBeNull();
        review!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task AddReviewAsync_DuplicateReview_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", InstructorId = Guid.NewGuid() });
        context.Enrollments.Add(new Enrollment { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, Status = "Completed" });
        context.CourseReviews.Add(new CourseReview { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, Rating = 4, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Another review" };

        // Act
        var result = await service.AddReviewAsync(userId, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddReviewAsync_NotEnrolled_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", InstructorId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Review" };

        // Act
        var result = await service.AddReviewAsync(userId, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddReviewAsync_NotCompleted_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", InstructorId = Guid.NewGuid() });
        context.Enrollments.Add(new Enrollment { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, Status = "InProgress" });
        await context.SaveChangesAsync();

        var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Review" };

        // Act
        var result = await service.AddReviewAsync(userId, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddReviewAsync_InstructorReviewingOwnCourse_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);
        var instructorId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", InstructorId = instructorId });
        context.Enrollments.Add(new Enrollment { Id = Guid.NewGuid(), UserId = instructorId, CourseId = courseId, Status = "Completed" });
        await context.SaveChangesAsync();

        var request = new ReviewRequest { CourseId = courseId, Rating = 5, Comment = "Self review" };

        // Act
        var result = await service.AddReviewAsync(instructorId, request);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetCourseReviewsAsync Tests

    [Fact]
    public async Task GetCourseReviewsAsync_WithReviews_ShouldReturnAll()
    {
        // Arrange
        using var context = GetDbContext();
        var courseId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), Username = "reviewer", Email = "r@test.com", PasswordHash = "hash" };
        context.Users.Add(user);
        context.CourseReviews.AddRange(
            new CourseReview { Id = Guid.NewGuid(), CourseId = courseId, UserId = user.Id, Rating = 5, CreatedAt = DateTime.UtcNow, User = user },
            new CourseReview { Id = Guid.NewGuid(), CourseId = courseId, UserId = user.Id, Rating = 4, CreatedAt = DateTime.UtcNow.AddDays(-1), User = user }
        );
        await context.SaveChangesAsync();
        var service = new ReviewService(context);

        // Act
        var result = await service.GetCourseReviewsAsync(courseId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCourseReviewsAsync_NoReviews_ShouldReturnEmpty()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);

        // Act
        var result = await service.GetCourseReviewsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetRatingSummaryAsync Tests

    [Fact]
    public async Task GetRatingSummaryAsync_WithReviews_ShouldCalculateCorrectly()
    {
        // Arrange
        using var context = GetDbContext();
        var courseId = Guid.NewGuid();
        context.CourseReviews.AddRange(
            new CourseReview { Id = Guid.NewGuid(), CourseId = courseId, UserId = Guid.NewGuid(), Rating = 5, CreatedAt = DateTime.UtcNow },
            new CourseReview { Id = Guid.NewGuid(), CourseId = courseId, UserId = Guid.NewGuid(), Rating = 3, CreatedAt = DateTime.UtcNow },
            new CourseReview { Id = Guid.NewGuid(), CourseId = courseId, UserId = Guid.NewGuid(), Rating = 4, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new ReviewService(context);

        // Act
        var summary = await service.GetRatingSummaryAsync(courseId);

        // Assert
        summary.TotalReviews.Should().Be(3);
        summary.AverageRating.Should().Be(4);  // (5+3+4)/3 = 4
    }

    [Fact]
    public async Task GetRatingSummaryAsync_NoReviews_ShouldReturnZeros()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);

        // Act
        var summary = await service.GetRatingSummaryAsync(Guid.NewGuid());

        // Assert
        summary.TotalReviews.Should().Be(0);
        summary.AverageRating.Should().Be(0);
    }

    #endregion

    #region HasUserReviewedAsync Tests

    [Fact]
    public async Task HasUserReviewedAsync_UserHasReviewed_ShouldReturnTrue()
    {
        // Arrange
        using var context = GetDbContext();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        context.CourseReviews.Add(new CourseReview { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, Rating = 5, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var service = new ReviewService(context);

        // Act
        var result = await service.HasUserReviewedAsync(userId, courseId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasUserReviewedAsync_UserHasNotReviewed_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new ReviewService(context);

        // Act
        var result = await service.HasUserReviewedAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
