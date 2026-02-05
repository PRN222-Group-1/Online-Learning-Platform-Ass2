using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using FluentAssertions;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class NotificationServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    #region SendNotificationAsync Tests

    [Fact]
    public async Task SendNotificationAsync_ValidData_ShouldCreateNotification()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new NotificationService(context);
        var userId = Guid.NewGuid();

        // Act
        await service.SendNotificationAsync(userId, "Test message", "General");

        // Assert
        var notification = await context.Notifications.FirstOrDefaultAsync();
        notification.Should().NotBeNull();
        notification!.UserId.Should().Be(userId);
        notification.Message.Should().Be("Test message");
        notification.Type.Should().Be("General");
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task SendNotificationAsync_DefaultType_ShouldUseGeneral()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new NotificationService(context);
        var userId = Guid.NewGuid();

        // Act
        await service.SendNotificationAsync(userId, "Test message");

        // Assert
        var notification = await context.Notifications.FirstOrDefaultAsync();
        notification!.Type.Should().Be("General");
    }

    [Fact]
    public async Task SendNotificationAsync_EmptyMessage_ShouldStillCreate()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new NotificationService(context);
        var userId = Guid.NewGuid();

        // Act
        await service.SendNotificationAsync(userId, "", "Alert");

        // Assert
        var notification = await context.Notifications.FirstOrDefaultAsync();
        notification.Should().NotBeNull();
        notification!.Message.Should().BeEmpty();
    }

    #endregion

    #region GetUserNotificationsAsync Tests

    [Fact]
    public async Task GetUserNotificationsAsync_AllNotifications_ShouldReturnAll()
    {
        // Arrange
        using var context = GetDbContext();
        var userId = Guid.NewGuid();
        context.Notifications.AddRange(
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "Msg1", IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "Msg2", IsRead = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new NotificationService(context);

        // Act
        var result = await service.GetUserNotificationsAsync(userId, onlyUnread: false);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_OnlyUnread_ShouldFilterCorrectly()
    {
        // Arrange
        using var context = GetDbContext();
        var userId = Guid.NewGuid();
        context.Notifications.AddRange(
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "Msg1", IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "Msg2", IsRead = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new NotificationService(context);

        // Act
        var result = await service.GetUserNotificationsAsync(userId, onlyUnread: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Message.Should().Be("Msg1");
    }

    [Fact]
    public async Task GetUserNotificationsAsync_NoNotifications_ShouldReturnEmpty()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new NotificationService(context);

        // Act
        var result = await service.GetUserNotificationsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldReturnOrderedByCreatedAt()
    {
        // Arrange
        using var context = GetDbContext();
        var userId = Guid.NewGuid();
        context.Notifications.AddRange(
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "Older", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "Newer", IsRead = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new NotificationService(context);

        // Act
        var result = (await service.GetUserNotificationsAsync(userId)).ToList();

        // Assert
        result.First().Message.Should().Be("Newer");
    }

    #endregion

    #region MarkAsReadAsync Tests

    [Fact]
    public async Task MarkAsReadAsync_ExistingNotification_ShouldMarkAsRead()
    {
        // Arrange
        using var context = GetDbContext();
        var notification = new Notification { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Message = "Test", IsRead = false, CreatedAt = DateTime.UtcNow };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        var service = new NotificationService(context);

        // Act
        var result = await service.MarkAsReadAsync(notification.Id);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Notifications.FindAsync(notification.Id);
        updated!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsReadAsync_NonExistentNotification_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new NotificationService(context);

        // Act
        var result = await service.MarkAsReadAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsReadAsync_AlreadyRead_ShouldStillReturnTrue()
    {
        // Arrange
        using var context = GetDbContext();
        var notification = new Notification { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Message = "Test", IsRead = true, CreatedAt = DateTime.UtcNow };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        var service = new NotificationService(context);

        // Act
        var result = await service.MarkAsReadAsync(notification.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetUnreadCountAsync Tests

    [Fact]
    public async Task GetUnreadCountAsync_WithUnreadNotifications_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = GetDbContext();
        var userId = Guid.NewGuid();
        context.Notifications.AddRange(
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "1", IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "2", IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "3", IsRead = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new NotificationService(context);

        // Act
        var count = await service.GetUnreadCountAsync(userId);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetUnreadCountAsync_NoNotifications_ShouldReturnZero()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new NotificationService(context);

        // Act
        var count = await service.GetUnreadCountAsync(Guid.NewGuid());

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetUnreadCountAsync_AllRead_ShouldReturnZero()
    {
        // Arrange
        using var context = GetDbContext();
        var userId = Guid.NewGuid();
        context.Notifications.AddRange(
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "1", IsRead = true, CreatedAt = DateTime.UtcNow },
            new Notification { Id = Guid.NewGuid(), UserId = userId, Message = "2", IsRead = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new NotificationService(context);

        // Act
        var count = await service.GetUnreadCountAsync(userId);

        // Assert
        count.Should().Be(0);
    }

    #endregion
}
