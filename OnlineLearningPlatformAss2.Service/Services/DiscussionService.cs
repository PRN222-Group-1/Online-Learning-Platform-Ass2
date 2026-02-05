using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.DTOs.Discussion;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class DiscussionService : IDiscussionService
{
    private readonly OnlineLearningContext _context;

    public DiscussionService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommentViewModel>> GetLessonCommentsAsync(Guid lessonId)
    {
        var comments = await _context.LessonComments
            .Include(c => c.User)
            .ThenInclude(u => u.Role)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .ThenInclude(u => u.Role)
            .Where(c => c.LessonId == lessonId && c.ParentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => MapToViewModel(c)).ToList();
    }

    public async Task<CommentViewModel> PostCommentAsync(Guid userId, CommentRequest request)
    {
        var comment = new LessonComment
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            UserId = userId,
            Content = request.Content,
            ParentId = request.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.LessonComments.Add(comment);
        await _context.SaveChangesAsync();

        // Reload to get user info
        var savedComment = await _context.LessonComments
            .Include(c => c.User)
            .ThenInclude(u => u.Role)
            .FirstAsync(c => c.Id == comment.Id);

        return MapToViewModel(savedComment);
    }

    public async Task<bool> DeleteCommentAsync(Guid userId, Guid commentId)
    {
        var comment = await _context.LessonComments.FindAsync(commentId);
        if (comment == null) return false;

        // Only author or admin can delete
        if (comment.UserId != userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstAsync(u => u.Id == userId);
            if (user.Role?.Name != "Admin") return false;
        }

        _context.LessonComments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    private CommentViewModel MapToViewModel(LessonComment comment)
    {
        return new CommentViewModel
        {
            Id = comment.Id,
            Content = comment.Content,
            Username = comment.User.Username,
            AvatarUrl = comment.User.Profile?.AvatarUrl,
            CreatedAt = comment.CreatedAt,
            IsInstructor = comment.User.Role?.Name == "Instructor" || comment.User.Role?.Name == "Admin",
            Replies = comment.Replies.Select(r => MapToViewModel(r)).ToList()
        };
    }
}
