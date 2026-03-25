using OnlineLearningPlatformAss2.Data.Entities;
using OnlineLearningPlatformAss2.Data.Repositories.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Order;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class OrderService(
    IOrderRepository orderRepository,
    INotificationService notificationService) : IOrderService
{
    public async Task<IEnumerable<OrderViewModel>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await orderRepository.GetUserOrdersAsync(userId);
        return orders.Select(o => new OrderViewModel
        {
            Id = o.OrderId,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            CreatedAt = o.CreatedAt
        });
    }

    public async Task<OrderDetailViewModel?> GetOrderDetailsAsync(Guid orderId)
    {
        var order = await orderRepository.GetOrderDetailsAsync(orderId);
        if (order == null) return null;

        var viewModel = new OrderDetailViewModel
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Transactions = order.Transactions.Select(t => new TransactionViewModel
            {
                Id = t.TransactionId,
                Amount = t.Amount,
                Status = t.Status,
                PaymentMethod = t.PaymentMethod,
                CreatedAt = t.CreatedAt
            }).ToList()
        };

        if (order.Course != null)
        {
            viewModel.Items.Add(new OrderItemViewModel
            {
                Id = order.Course.CourseId,
                Type = "Course",
                Title = order.Course.Title,
                Price = order.Course.Price,
                ImageUrl = order.Course.ImageUrl
            });
        }
        else if (order.Path != null)
        {
            viewModel.Items.Add(new OrderItemViewModel
            {
                Id = order.Path.PathId,
                Type = "LearningPath",
                Title = order.Path.Title,
                Price = order.Path.Price,
                // ImageUrl for LearningPath if exists, otherwise null
            });
        }

        return viewModel;
    }

    public async Task<OrderViewModel> CreateCourseOrderAsync(Guid userId, Guid courseId)
    {
        var course = await orderRepository.GetCourseByIdAsync(courseId);
        if (course == null)
            throw new InvalidOperationException("Course not found");

        var existingEnrollment = await orderRepository.IsEnrolledInCourseAsync(userId, courseId);
        if (existingEnrollment)
            throw new InvalidOperationException("User is already enrolled in this course");

        if (course.InstructorId == userId)
            throw new InvalidOperationException("You cannot purchase your own course.");

        var existingOrder = await orderRepository.HasPendingCourseOrderAsync(userId, courseId);
        if (existingOrder)
            throw new InvalidOperationException("You already have a pending order for this course. Please complete it in your checkout page.");

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            TotalAmount = course.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        await orderRepository.AddOrderAsync(order);
        await orderRepository.SaveChangesAsync();

        return new OrderViewModel
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<OrderViewModel> CreateLearningPathOrderAsync(Guid userId, Guid pathId)
    {
        var learningPath = await orderRepository.GetLearningPathByIdAsync(pathId);
        if (learningPath == null)
            throw new InvalidOperationException("Learning path not found");

        var existingEnrollment = await orderRepository.IsEnrolledInPathAsync(userId, pathId);
        if (existingEnrollment)
            throw new InvalidOperationException("User is already enrolled in this learning path");

        var existingOrder = await orderRepository.HasPendingPathOrderAsync(userId, pathId);
        if (existingOrder)
            throw new InvalidOperationException("You already have a pending order for this learning path.");

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = userId,
            PathId = pathId,
            TotalAmount = learningPath.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        await orderRepository.AddOrderAsync(order);
        await orderRepository.SaveChangesAsync();

        return new OrderViewModel
        {
            Id = order.OrderId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<bool> ProcessPaymentAsync(Guid orderId, string paymentMethod)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        try
        {
            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                OrderId = orderId,
                Amount = order.TotalAmount,
                Status = "Completed",
                PaymentMethod = paymentMethod,
                CreatedAt = DateTime.UtcNow
            };

            await orderRepository.AddTransactionAsync(transaction);
            order.Status = "Completed";

            if (order.CourseId.HasValue)
            {
                var exists = await orderRepository.IsEnrolledInCourseAsync(order.UserId, order.CourseId.Value);
                if (!exists)
                {
                    await orderRepository.AddEnrollmentAsync(new Enrollment
                    {
                        EnrollmentId = Guid.NewGuid(),
                        UserId = order.UserId,
                        CourseId = order.CourseId.Value,
                        EnrolledAt = DateTime.UtcNow,
                        Status = "Active"
                    });

                    var course = await orderRepository.GetCourseByIdAsync(order.CourseId.Value);
                    if (course != null)
                    {
                        var student = await orderRepository.GetUserByIdAsync(order.UserId);
                        await notificationService.SendNotificationAsync(
                            course.InstructorId,
                            $"New Enrollment! Student {student?.Username} has joined your course '{course.Title}'.",
                            "Enrollment"
                        );
                    }
                }
            }
            else if (order.PathId.HasValue)
            {
                var pathId = order.PathId.Value;
                var exists = await orderRepository.IsEnrolledInPathAsync(order.UserId, pathId);
                if (!exists)
                {
                    await orderRepository.AddPathEnrollmentAsync(new UserLearningPathEnrollment
                    {
                        EnrollmentId = Guid.NewGuid(),
                        UserId = order.UserId,
                        PathId = pathId,
                        EnrolledAt = DateTime.UtcNow,
                        Status = "Active",
                        ProgressPercentage = 0
                    });

                    var courseIds = await orderRepository.GetPathCourseIdsAsync(pathId);
                    foreach (var courseId in courseIds)
                    {
                        var courseEnrolled = await orderRepository.IsEnrolledInCourseAsync(order.UserId, courseId);
                        if (!courseEnrolled)
                        {
                            await orderRepository.AddEnrollmentAsync(new Enrollment
                            {
                                EnrollmentId = Guid.NewGuid(),
                                UserId = order.UserId,
                                CourseId = courseId,
                                EnrolledAt = DateTime.UtcNow,
                                Status = "Active"
                            });

                            var course = await orderRepository.GetCourseByIdAsync(courseId);
                            if (course != null)
                            {
                                var student = await orderRepository.GetUserByIdAsync(order.UserId);
                                await notificationService.SendNotificationAsync(
                                    course.InstructorId,
                                    $"Path Enrollment! Student {student?.Username} has joined your course '{course.Title}' via a Learning Path.",
                                    "Enrollment"
                                );
                            }
                        }
                    }
                }
            }

            await orderRepository.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<OrderStatsViewModel> GetOrderStatsAsync(Guid userId)
    {
        var orders = (await orderRepository.GetUserOrdersAsync(userId)).ToList();

        return new OrderStatsViewModel
        {
            TotalOrders = orders.Count,
            TotalSpent = orders.Where(o => o.Status == "Completed").Sum(o => o.TotalAmount),
            CompletedOrders = orders.Count(o => o.Status == "Completed"),
            PendingOrders = orders.Count(o => o.Status == "Pending")
        };
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null || order.Status != "Pending") return false;

        order.Status = "Cancelled";
        await orderRepository.SaveChangesAsync();
        return true;
    }
}
