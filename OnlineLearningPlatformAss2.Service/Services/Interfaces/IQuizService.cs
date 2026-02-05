using OnlineLearningPlatformAss2.Service.DTOs.Quiz;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IQuizService
{
    Task<QuizViewModel?> GetQuizForLessonAsync(Guid lessonId);
    Task<QuizResultDto> SubmitAttemptAsync(Guid userId, QuizSubmissionDto submission);
    Task<bool> HasPassedQuizAsync(Guid userId, Guid quizId);
}
