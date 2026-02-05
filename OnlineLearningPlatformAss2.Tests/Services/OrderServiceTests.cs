using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using FluentAssertions;
using Xunit;
using Moq;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class OrderServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    private Mock<INotificationService> GetMockNotificationService() => new Mock<INotificationService>();

    #region GetUserOrdersAsync Tests

    [Fact]
    public async Task GetUserOrdersAsync_WithOrders_ShouldReturnAll()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        context.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 50, Status = "Completed", CreatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 30, Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );
        await context.SaveChangesAsync();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.GetUserOrdersAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserOrdersAsync_NoOrders_ShouldReturnEmpty()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.GetUserOrdersAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserOrdersAsync_OrderedByCreatedAtDescending()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        context.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 50, Status = "Old", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 30, Status = "New", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.GetUserOrdersAsync(userId);

        // Assert
        result.First().Status.Should().Be("New");
    }

    #endregion

    #region GetOrderDetailsAsync Tests

    [Fact]
    public async Task GetOrderDetailsAsync_ExistingOrder_ShouldReturnDetails()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        context.Orders.Add(new Order { Id = orderId, UserId = userId, TotalAmount = 100, Status = "Completed", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.GetOrderDetailsAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.TotalAmount.Should().Be(100);
    }

    [Fact]
    public async Task GetOrderDetailsAsync_NonExistentOrder_ShouldReturnNull()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.GetOrderDetailsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateCourseOrderAsync Tests

    [Fact]
    public async Task CreateCourseOrderAsync_ValidCourse_ShouldCreateOrder()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", Price = 49.99m, Status = "Published", InstructorId = Guid.NewGuid() });
        await context.SaveChangesAsync();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.CreateCourseOrderAsync(userId, courseId);

        // Assert
        result.Should().NotBeNull();
        result!.TotalAmount.Should().Be(49.99m);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CreateCourseOrderAsync_AlreadyEnrolled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", Price = 49.99m, Status = "Published", InstructorId = Guid.NewGuid() });
        context.Enrollments.Add(new Enrollment { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, Status = "Active" });
        await context.SaveChangesAsync();
        var service = new OrderService(context, mockNotification.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.CreateCourseOrderAsync(userId, courseId));
        exception.Message.Should().Contain("already enrolled");
    }

    [Fact]
    public async Task CreateCourseOrderAsync_NonExistentCourse_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new OrderService(context, mockNotification.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.CreateCourseOrderAsync(Guid.NewGuid(), Guid.NewGuid()));
        exception.Message.Should().Contain("Course not found");
    }

    #endregion

    #region ProcessPaymentAsync Tests

    [Fact]
    public async Task ProcessPaymentAsync_ValidPayment_ShouldCompleteOrder()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", Price = 50, Status = "Published", InstructorId = Guid.NewGuid() });
        context.Orders.Add(new Order { Id = orderId, UserId = userId, CourseId = courseId, TotalAmount = 50, Status = "Pending", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.ProcessPaymentAsync(orderId, "CreditCard");

        // Assert
        result.Should().BeTrue();
        var order = await context.Orders.FindAsync(orderId);
        order!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task ProcessPaymentAsync_NonExistentOrder_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var result = await service.ProcessPaymentAsync(Guid.NewGuid(), "CreditCard");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldCreateEnrollment()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        context.Courses.Add(new Course { Id = courseId, Title = "Test", Description = "Desc", Price = 50, Status = "Published", InstructorId = Guid.NewGuid() });
        context.Orders.Add(new Order { Id = orderId, UserId = userId, CourseId = courseId, TotalAmount = 50, Status = "Pending", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        
        var service = new OrderService(context, mockNotification.Object);

        // Act
        await service.ProcessPaymentAsync(orderId, "CreditCard");

        // Assert
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        enrollment.Should().NotBeNull();
        enrollment!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldCreateTransaction()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        context.Orders.Add(new Order { Id = orderId, UserId = userId, TotalAmount = 75, Status = "Pending", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        
        var service = new OrderService(context, mockNotification.Object);

        // Act
        await service.ProcessPaymentAsync(orderId, "PayPal");

        // Assert
        var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.OrderId == orderId);
        transaction.Should().NotBeNull();
        transaction!.PaymentMethod.Should().Be("PayPal");
        transaction.Amount.Should().Be(75);
    }

    #endregion

    #region GetOrderStatsAsync Tests

    [Fact]
    public async Task GetOrderStatsAsync_WithOrders_ShouldCalculateCorrectly()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var userId = Guid.NewGuid();
        context.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 100, Status = "Completed", CreatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 50, Status = "Completed", CreatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 30, Status = "Pending", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var stats = await service.GetOrderStatsAsync(userId);

        // Assert
        stats.TotalOrders.Should().Be(3);
        stats.CompletedOrders.Should().Be(2);
        stats.TotalSpent.Should().Be(150); // Only completed orders
    }

    [Fact]
    public async Task GetOrderStatsAsync_NoOrders_ShouldReturnZeros()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new OrderService(context, mockNotification.Object);

        // Act
        var stats = await service.GetOrderStatsAsync(Guid.NewGuid());

        // Assert
        stats.TotalOrders.Should().Be(0);
        stats.TotalSpent.Should().Be(0);
    }

    #endregion
}
