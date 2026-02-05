using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Quiz;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Pages;

public class LearnPageTests
{
    private readonly Mock<ICourseService> _courseServiceMock;
    private readonly Mock<IQuizService> _quizServiceMock;
    private readonly Mock<IDiscussionService> _discussionServiceMock;
    private readonly Mock<IReviewService> _reviewServiceMock;
    private readonly LearnModel _pageModel;

    public LearnPageTests()
    {
        _courseServiceMock = new Mock<ICourseService>();
        _quizServiceMock = new Mock<IQuizService>();
        _discussionServiceMock = new Mock<IDiscussionService>();
        _reviewServiceMock = new Mock<IReviewService>();
        
        _pageModel = new LearnModel(_courseServiceMock.Object, _quizServiceMock.Object, _discussionServiceMock.Object, _reviewServiceMock.Object);
        
        // Setup HttpContext for User identity
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        }, "TestAuth"));

        _pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task OnPostCompleteLessonAsync_ShouldReturnJsonSuccess_WhenServiceSucceeds()
    {
        // Arrange
        var request = new LearnModel.CompleteLessonRequest
        {
            CourseId = Guid.NewGuid(),
            LessonId = Guid.NewGuid()
        };
        
        _courseServiceMock.Setup(s => s.UpdateLessonProgressAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true))
            .ReturnsAsync(true);
        
        _courseServiceMock.Setup(s => s.GetEnrollmentIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _pageModel.OnPostCompleteLessonAsync(request);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = (JsonResult)result;
        jsonResult.Value.Should().BeEquivalentTo(new { success = true });
    }

    [Fact]
    public async Task OnGetQuizAsync_ShouldReturnQuizData()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var quizData = new QuizViewModel { Title = "Test Quiz", Questions = new List<QuestionViewModel>() };
        
        _quizServiceMock.Setup(s => s.GetQuizForLessonAsync(lessonId))
            .ReturnsAsync(quizData);

        // Act
        var result = await _pageModel.OnGetQuizAsync(lessonId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = (JsonResult)result;
        jsonResult.Value.Should().Be(quizData);
    }
}
