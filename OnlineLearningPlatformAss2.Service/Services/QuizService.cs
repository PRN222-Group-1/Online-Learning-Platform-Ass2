using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.DTOs.Quiz;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class QuizService : IQuizService
{
    private readonly OnlineLearningContext _context;

    public QuizService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<QuizViewModel?> GetQuizForLessonAsync(Guid lessonId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.LessonId == lessonId);

        if (quiz == null) return null;

        return new QuizViewModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Questions = quiz.Questions.Select(q => new QuestionViewModel
            {
                Id = q.Id,
                Text = q.Text,
                Options = q.Options.Select(o => new OptionViewModel
                {
                    Id = o.Id,
                    Text = o.Text
                }).ToList()
            }).ToList()
        };
    }

    public async Task<QuizResultDto> SubmitAttemptAsync(Guid userId, QuizSubmissionDto submission)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == submission.QuizId);

        if (quiz == null) throw new ArgumentException("Quiz not found");

        int correctCount = 0;
        foreach (var answer in submission.Answers)
        {
            var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question != null)
            {
                var originalOption = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId);
                if (originalOption != null && originalOption.IsCorrect)
                {
                    correctCount++;
                }
            }
        }

        int score = (int)Math.Round((double)correctCount / quiz.Questions.Count * 100);
        
        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            QuizId = quiz.Id,
            Score = score,
            AttemptedAt = DateTime.UtcNow
        };

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return new QuizResultDto
        {
            Score = score,
            TotalQuestions = quiz.Questions.Count,
            Passed = score >= 80 // Pass threshold 80%
        };
    }

    public async Task<bool> HasPassedQuizAsync(Guid userId, Guid quizId)
    {
        return await _context.QuizAttempts.AnyAsync(a => a.UserId == userId && a.QuizId == quizId && a.Score >= 80);
    }
}
