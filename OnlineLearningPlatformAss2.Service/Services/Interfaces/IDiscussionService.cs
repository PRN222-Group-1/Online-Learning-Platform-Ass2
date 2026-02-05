using OnlineLearningPlatformAss2.Service.DTOs.Discussion;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IDiscussionService
{
    Task<IEnumerable<CommentViewModel>> GetLessonCommentsAsync(Guid lessonId);
    Task<CommentViewModel> PostCommentAsync(Guid userId, CommentRequest request);
    Task<bool> DeleteCommentAsync(Guid userId, Guid commentId);
}
