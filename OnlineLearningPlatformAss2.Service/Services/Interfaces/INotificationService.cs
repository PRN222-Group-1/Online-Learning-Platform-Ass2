using OnlineLearningPlatformAss2.Data.Database.Entities;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string message, string type = "General");
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool onlyUnread = false);
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<int> GetUnreadCountAsync(Guid userId);
}
