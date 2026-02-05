using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using OnlineLearningPlatformAss2.Service.DTOs.Quiz;
using FluentAssertions;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class QuizServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldReturnPass_WhenScoreIsAbove80()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new QuizService(context);
        var quiz = new Quiz { Id = Guid.NewGuid(), LessonId = Guid.NewGuid(), Title = "Test Quiz" };
        var question = new Question { Id = Guid.NewGuid(), QuizId = quiz.Id, Text = "Q1" };
        var optionCorrect = new Option { Id = Guid.NewGuid(), QuestionId = question.Id, Text = "Correct", IsCorrect = true };
        var optionWrong = new Option { Id = Guid.NewGuid(), QuestionId = question.Id, Text = "Wrong", IsCorrect = false };
        
        context.Quizzes.Add(quiz);
        context.Questions.Add(question);
        context.Options.AddRange(optionCorrect, optionWrong);
        await context.SaveChangesAsync();

        var submission = new QuizSubmissionDto
        {
            QuizId = quiz.Id,
            Answers = new List<AnswerSubmissionDto>
            {
                new() { QuestionId = question.Id, SelectedOptionId = optionCorrect.Id }
            }
        };

        // Act
        var result = await service.SubmitAttemptAsync(Guid.NewGuid(), submission);

        // Assert
        result.Passed.Should().BeTrue();
        result.Score.Should().Be(100);
    }
}
