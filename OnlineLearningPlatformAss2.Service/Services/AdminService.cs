using OnlineLearningPlatformAss2.Data.Entities;
using OnlineLearningPlatformAss2.Data.Repositories.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using BCrypt.Net;

namespace OnlineLearningPlatformAss2.Service.Services;

public class AdminService(
    IAdminRepository adminRepository,
    IUserRepository userRepository,
    ICourseRepository courseRepository,
    INotificationService notificationService,
    ICourseUpdateBroadcaster? broadcaster = null) : IAdminService
{
    public async Task<AdminStatsDto> GetStatsAsync()
    {
        var stats = new AdminStatsDto
        {
            TotalUsers = await adminRepository.GetUserCountAsync(),
            TotalInstructors = await adminRepository.GetInstructorCountAsync(),
            TotalCourses = await adminRepository.GetCourseCountAsync(),
            PendingCourses = await adminRepository.GetPendingCourseCountAsync(),
            TotalEnrollments = await adminRepository.GetEnrollmentCountAsync(),
            TotalRevenue = await adminRepository.GetTotalRevenueAsync()
        };
        stats.TotalNetProfit = stats.TotalRevenue * 0.30m;

        var recentOrders = await adminRepository.GetRecentOrdersAsync(5);
        stats.RecentOrders = recentOrders.Select(o => new RecentOrderDto
        {
            OrderId = o.OrderId,
            Username = o.User.Username,
            ItemTitle = o.Course != null ? o.Course.Title : (o.Path != null ? o.Path.Title : "Bundle"),
            Amount = o.TotalAmount,
            CreatedAt = o.CreatedAt
        }).ToList();

        return stats;
    }

    public async Task<IEnumerable<CourseViewModel>> GetPendingCoursesAsync()
    {
        var courses = await adminRepository.GetPendingCoursesAsync();
        return courses.Select(c => new CourseViewModel
        {
            Id = c.CourseId,
            Title = c.Title,
            Price = c.Price,
            CategoryName = c.Category.Name,
            InstructorName = c.Instructor.Username,
            ImageUrl = c.ImageUrl,
            Status = c.Status,
            RejectionReason = c.RejectionReason
        });
    }

    public async Task<bool> ApproveCourseAsync(Guid courseId)
    {
        var course = await adminRepository.GetCourseByIdAsync(courseId);
        if (course == null) return false;

        course.Status = "Published";
        course.RejectionReason = null;
        await adminRepository.UpdateCourseAsync(course);
        await adminRepository.SaveChangesAsync();

        await notificationService.SendNotificationAsync(
            course.InstructorId,
            $"Congratulations! Your course '{course.Title}' has been approved and is now live.",
            "Approval"
        );

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(courseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> RejectCourseAsync(Guid courseId, string reason)
    {
        var course = await adminRepository.GetCourseByIdAsync(courseId);
        if (course == null) return false;

        course.Status = "Rejected";
        course.RejectionReason = reason;
        await adminRepository.UpdateCourseAsync(course);
        await adminRepository.SaveChangesAsync();

        await notificationService.SendNotificationAsync(
            course.InstructorId,
            $"Action Required: Your course submission '{course.Title}' was rejected. Reason: {reason}",
            "Rejection"
        );

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(courseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> SuspendCourseAsync(Guid courseId)
    {
        var course = await adminRepository.GetCourseByIdAsync(courseId);
        if (course == null) return false;

        course.Status = "Suspended";
        await adminRepository.UpdateCourseAsync(course);
        await adminRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(courseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> UnsuspendCourseAsync(Guid courseId)
    {
        var course = await adminRepository.GetCourseByIdAsync(courseId);
        if (course == null) return false;

        course.Status = "Published";
        await adminRepository.UpdateCourseAsync(course);
        await adminRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(courseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync()
    {
        var courses = await adminRepository.GetAllCoursesAsync();
        var viewModels = new List<CourseViewModel>();
        
        foreach (var c in courses)
        {
            viewModels.Add(new CourseViewModel
            {
                Id = c.CourseId,
                Title = c.Title,
                Price = c.Price,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                ImageUrl = c.ImageUrl,
                Status = c.Status,
                StudentCount = await courseRepository.GetEnrollmentCountAsync(c.CourseId)
            });
        }
        
        return viewModels;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string? searchTerm = null)
    {
        var users = await userRepository.SearchUsersAsync(searchTerm);
        return users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            RoleName = u.Role?.Name ?? "User",
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<bool> ToggleUserStatusAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, string roleName)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        var role = await userRepository.GetRoleByNameAsync(roleName);
        if (role == null) return false;

        user.RoleId = role.Id;
        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await userRepository.GetUserWithRoleAsync(userId);
        if (user == null || user.Username == "admin") return false;

        if (user.Role?.Name == "Instructor")
        {
            var hasStudents = await adminRepository.HasActiveStudentsAsync(userId);
            if (hasStudents) return false;
        }

        await userRepository.RemoveAsync(user);
        await userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddInternalUserAsync(string username, string email, string password, string roleName)
    {
        var role = await userRepository.GetRoleByNameAsync(roleName);
        if (role == null) return false;

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = hashedPassword,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();
        return true;
    }

    private async Task<CourseViewModel?> GetCourseViewModelAsync(Guid courseId)
    {
        var course = await adminRepository.GetCourseWithDetailsAsync(courseId);
        if (course == null) return null;
        
        return new CourseViewModel
        {
            Id = course.CourseId,
            Title = course.Title,
            Price = course.Price,
            CategoryName = course.Category.Name,
            InstructorName = course.Instructor.Username,
            ImageUrl = course.ImageUrl,
            Status = course.Status,
            RejectionReason = course.RejectionReason
        };
    }
}
