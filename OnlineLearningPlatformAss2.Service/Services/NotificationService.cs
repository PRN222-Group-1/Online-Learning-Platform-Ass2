using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class NotificationService : INotificationService
{
    private readonly OnlineLearningContext _context;

    public NotificationService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task SendNotificationAsync(Guid userId, string message, string type = "General")
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool onlyUnread = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .AsQueryable();

        if (onlyUnread)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query.ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
