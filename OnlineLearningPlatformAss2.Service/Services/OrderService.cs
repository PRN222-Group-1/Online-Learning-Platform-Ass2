using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.DTOs.Order;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class OrderService : IOrderService
{
    private readonly OnlineLearningContext _context;
    private readonly INotificationService _notificationService;

    public OrderService(OnlineLearningContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<OrderViewModel>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                CompletedAt = o.CompletedAt
            })
            .ToListAsync();

        return orders;
    }

    public async Task<OrderDetailViewModel?> GetOrderDetailsAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId)
            .Select(o => new OrderDetailViewModel
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                CompletedAt = o.CompletedAt,
                Transactions = o.Transactions.Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Status = t.Status,
                    PaymentMethod = t.PaymentMethod,
                    CreatedAt = t.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return order;
    }

    public async Task<OrderViewModel> CreateCourseOrderAsync(Guid userId, Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
            throw new InvalidOperationException("Course not found");

        // Check if user is already enrolled
        var existingEnrollment = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (existingEnrollment)
            throw new InvalidOperationException("User is already enrolled in this course");

        if (course.InstructorId == userId)
            throw new InvalidOperationException("You cannot purchase your own course.");

        // Check for existing pending orders
        var existingOrder = await _context.Orders
            .AnyAsync(o => o.UserId == userId && o.CourseId == courseId && o.Status == "Pending");

        if (existingOrder)
            throw new InvalidOperationException("You already have a pending order for this course. Please complete it in your checkout page.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            TotalAmount = course.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new OrderViewModel
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            CompletedAt = order.CompletedAt
        };
    }

    public async Task<OrderViewModel> CreateLearningPathOrderAsync(Guid userId, Guid pathId)
    {
        var learningPath = await _context.LearningPaths.FindAsync(pathId);
        if (learningPath == null)
            throw new InvalidOperationException("Learning path not found");

        // Check if user is already enrolled
        var existingEnrollment = await _context.UserLearningPathEnrollments
            .AnyAsync(e => e.UserId == userId && e.PathId == pathId);

        if (existingEnrollment)
            throw new InvalidOperationException("User is already enrolled in this learning path");

        // Check for existing pending orders
        var existingOrder = await _context.Orders
            .AnyAsync(o => o.UserId == userId && o.LearningPathId == pathId && o.Status == "Pending");

        if (existingOrder)
            throw new InvalidOperationException("You already have a pending order for this learning path.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LearningPathId = pathId,
            TotalAmount = learningPath.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new OrderViewModel
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            CompletedAt = order.CompletedAt
        };
    }

    public async Task<bool> ProcessPaymentAsync(Guid orderId, string paymentMethod)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return false;

        try
        {
            // Create transaction
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = order.TotalAmount,
                Status = "Completed", // In a real app, this would integrate with payment gateway
                PaymentMethod = paymentMethod,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Update order status
            order.Status = "Completed";
            order.CompletedAt = DateTime.UtcNow;

            // Handle automatic enrollment
            if (order.CourseId.HasValue)
            {
                var exists = await _context.Enrollments.AnyAsync(e => e.UserId == order.UserId && e.CourseId == order.CourseId.Value);
                if (!exists)
                {
                    _context.Enrollments.Add(new Enrollment
                    {
                        Id = Guid.NewGuid(),
                        UserId = order.UserId,
                        CourseId = order.CourseId.Value,
                        EnrolledAt = DateTime.UtcNow,
                        Status = "Active"
                    });

                    // Trigger instructor notification
                    var course = await _context.Courses.FindAsync(order.CourseId.Value);
                    if (course != null)
                    {
                        var student = await _context.Users.FindAsync(order.UserId);
                        await _notificationService.SendNotificationAsync(
                            course.InstructorId, 
                            $"New Enrollment! Student {student?.Username} has joined your course '{course.Title}'.",
                            "Enrollment"
                        );
                    }
                }
            }
            else if (order.LearningPathId.HasValue)
            {
                var pathId = order.LearningPathId.Value;
                var exists = await _context.UserLearningPathEnrollments.AnyAsync(e => e.UserId == order.UserId && e.PathId == pathId);
                if (!exists)
                {
                    _context.UserLearningPathEnrollments.Add(new UserLearningPathEnrollment
                    {
                        Id = Guid.NewGuid(),
                        UserId = order.UserId,
                        PathId = pathId,
                        EnrolledAt = DateTime.UtcNow,
                        Status = "Active"
                    });

                    // Also enroll in all courses within this path
                    var courseIds = await _context.PathCourses
                        .Where(pc => pc.PathId == pathId)
                        .Select(pc => pc.CourseId)
                        .ToListAsync();

                    foreach (var courseId in courseIds)
                    {
                        var courseEnrolled = await _context.Enrollments.AnyAsync(e => e.UserId == order.UserId && e.CourseId == courseId);
                        if (!courseEnrolled)
                        {
                            _context.Enrollments.Add(new Enrollment
                            {
                                Id = Guid.NewGuid(),
                                UserId = order.UserId,
                                CourseId = courseId,
                                EnrolledAt = DateTime.UtcNow,
                                Status = "Active"
                            });

                            // Notification for learning path course enrollment
                            var course = await _context.Courses.FindAsync(courseId);
                            if (course != null)
                            {
                                var student = await _context.Users.FindAsync(order.UserId);
                                await _notificationService.SendNotificationAsync(
                                    course.InstructorId,
                                    $"Path Enrollment! Student {student?.Username} has joined your course '{course.Title}' via a Learning Path.",
                                    "Enrollment"
                                );
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<OrderStatsViewModel> GetOrderStatsAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();

        return new OrderStatsViewModel
        {
            TotalOrders = orders.Count,
            TotalSpent = orders.Where(o => o.Status == "Completed").Sum(o => o.TotalAmount),
            CompletedOrders = orders.Count(o => o.Status == "Completed"),
            PendingOrders = orders.Count(o => o.Status == "Pending")
        };
    }
}
