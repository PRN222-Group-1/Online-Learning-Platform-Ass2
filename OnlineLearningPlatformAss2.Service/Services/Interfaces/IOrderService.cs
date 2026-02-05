using OnlineLearningPlatformAss2.Service.DTOs.Order;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IOrderService
{
    /// <summary>
    /// Get all orders for a specific user
    /// </summary>
    Task<IEnumerable<OrderViewModel>> GetUserOrdersAsync(Guid userId);

    /// <summary>
    /// Get order details by ID
    /// </summary>
    Task<OrderDetailViewModel?> GetOrderDetailsAsync(Guid orderId);

    /// <summary>
    /// Create a new order for course enrollment
    /// </summary>
    Task<OrderViewModel> CreateCourseOrderAsync(Guid userId, Guid courseId);

    /// <summary>
    /// Create a new order for learning path enrollment
    /// </summary>
    Task<OrderViewModel> CreateLearningPathOrderAsync(Guid userId, Guid pathId);

    /// <summary>
    /// Process payment for an order
    /// </summary>
    Task<bool> ProcessPaymentAsync(Guid orderId, string paymentMethod);

    /// <summary>
    /// Get order statistics for dashboard
    /// </summary>
    Task<OrderStatsViewModel> GetOrderStatsAsync(Guid userId);
}
