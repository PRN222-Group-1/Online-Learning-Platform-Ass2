using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using OnlineLearningPlatformAss2.Service.DTOs.Discussion;
using FluentAssertions;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class DiscussionServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    [Fact]
    public async Task PostCommentAsync_ShouldCreateComment_AndLinkToUser()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new DiscussionService(context);
        var user = new User { Id = Guid.NewGuid(), Username = "talker", Email = "talk@test.com", PasswordHash = "hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var request = new CommentRequest
        {
            LessonId = Guid.NewGuid(),
            Content = "Test comment"
        };

        // Act
        var result = await service.PostCommentAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Test comment");
        result.Username.Should().Be("talker");
        
        var dbComment = await context.LessonComments.FirstOrDefaultAsync(c => c.Id == result.Id);
        dbComment.Should().NotBeNull();
        dbComment!.UserId.Should().Be(user.Id);
    }
}
