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
    ICourseUpdateBroadcaster? broadcaster = null,
    IAdminUpdateBroadcaster? adminBroadcaster = null) : IAdminService
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
                InstructorId = c.InstructorId,
                CategoryId = c.CategoryId,
                Description = c.Description ?? "",
                Level = c.Level,
                Language = c.Language,
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

        if (adminBroadcaster != null)
        {
            await adminBroadcaster.BroadcastUserStatusToggledAsync(userId, user.IsActive);
        }
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

        if (adminBroadcaster != null)
        {
            await adminBroadcaster.BroadcastUserRoleChangedAsync(userId, roleName);
        }
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

        if (adminBroadcaster != null)
        {
            await adminBroadcaster.BroadcastUserDeletedAsync(userId);
        }
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

        if (adminBroadcaster != null)
        {
            await adminBroadcaster.BroadcastUserAddedAsync(new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = roleName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }
        return true;
    }

    public async Task<bool> ResetUserPasswordAsync(Guid userId, string newPassword)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        if (adminBroadcaster != null)
        {
            await adminBroadcaster.BroadcastUserPasswordResetAsync(userId);
        }
        return true;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllInstructorsAsync()
    {
        var users = await userRepository.SearchUsersAsync(null);
        return users.Where(u => u.Role?.Name == "Instructor").Select(u => new AdminUserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            RoleName = "Instructor",
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<bool> CreateCourseAsync(CourseCreateDto dto)
    {
        var course = new Course
        {
            CourseId = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            InstructorId = dto.InstructorId,
            IsFeatured = dto.IsFeatured,
            Level = dto.Level,
            Language = dto.Language,
            Status = "Published", // Admin created courses are published by default
            CreatedAt = DateTime.UtcNow
        };

        await adminRepository.AddCourseAsync(course);
        await adminRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(course.CourseId);
            if (courseVm != null) await broadcaster.BroadcastCourseAddedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> UpdateCourseAsync(Guid courseId, CourseUpdateDto dto)
    {
        var course = await adminRepository.GetCourseByIdAsync(courseId);
        if (course == null) return false;

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.Price = dto.Price;
        course.ImageUrl = dto.ImageUrl;
        course.CategoryId = dto.CategoryId;
        course.IsFeatured = dto.IsFeatured;
        course.Level = dto.Level;
        course.Language = dto.Language;

        await adminRepository.UpdateCourseAsync(course);
        await adminRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(courseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> DeleteCourseAsync(Guid courseId)
    {
        var course = await adminRepository.GetCourseByIdAsync(courseId);
        if (course == null) return false;

        await adminRepository.DeleteCourseAsync(course);
        await adminRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            await broadcaster.BroadcastCourseDeletedAsync(courseId);
        }

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
            InstructorId = course.InstructorId,
            CategoryId = course.CategoryId,
            Description = course.Description ?? "",
            Level = course.Level,
            Language = course.Language,
            ImageUrl = course.ImageUrl,
            IsFeatured = course.IsFeatured,
            Status = course.Status,
            StudentCount = await courseRepository.GetEnrollmentCountAsync(course.CourseId),
            RejectionReason = course.RejectionReason
        };
    }

    public async Task<IEnumerable<CourseModuleDto>> GetCourseCurriculumAsync(Guid courseId)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(courseId);
        if (course == null) return Enumerable.Empty<CourseModuleDto>();

        return course.Modules.OrderBy(m => m.OrderIndex).Select(m => new CourseModuleDto
        {
            Id = m.ModuleId,
            Title = m.Title,
            Description = m.Description ?? "",
            OrderIndex = m.OrderIndex,
            Lessons = m.Lessons.OrderBy(l => l.OrderIndex).Select(l => new CourseLessonDto
            {
                Id = l.LessonId,
                Title = l.Title,
                Content = l.Content ?? "",
                VideoUrl = l.VideoUrl,
                Duration = l.Duration ?? 0,
                OrderIndex = l.OrderIndex,
                IsPreview = false // Default for admin view
            }).ToList()
        });
    }

    public async Task<bool> AddModuleAsync(Guid courseId, string title, string description, int orderIndex)
    {
        var module = new Module
        {
            ModuleId = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Description = description,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };

        await courseRepository.AddModuleAsync(module);
        await courseRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(courseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> UpdateModuleAsync(Guid moduleId, string title, string description, int orderIndex)
    {
        var module = await courseRepository.GetModuleByIdAsync(moduleId);
        if (module == null) return false;

        module.Title = title;
        module.Description = description;
        module.OrderIndex = orderIndex;

        await courseRepository.UpdateModuleAsync(module);
        await courseRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(module.CourseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> DeleteModuleAsync(Guid moduleId)
    {
        var module = await courseRepository.GetModuleByIdAsync(moduleId);
        if (module == null) return false;

        var courseId = module.CourseId;
        await courseRepository.RemoveModuleAsync(module);
        await courseRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(courseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> AddLessonAsync(Guid moduleId, string title, string content, string? videoUrl, int duration, int orderIndex)
    {
        var module = await courseRepository.GetModuleByIdAsync(moduleId);
        if (module == null) return false;

        var lesson = new Lesson
        {
            LessonId = Guid.NewGuid(),
            ModuleId = moduleId,
            Title = title,
            Content = content,
            VideoUrl = videoUrl,
            Type = string.IsNullOrEmpty(videoUrl) ? "Text" : "Video",
            Duration = duration,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };

        await courseRepository.AddLessonAsync(lesson);
        await courseRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(module.CourseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> UpdateLessonAsync(Guid lessonId, string title, string content, string? videoUrl, int duration, int orderIndex)
    {
        var lesson = await courseRepository.GetLessonByIdAsync(lessonId);
        if (lesson == null) return false;

        var module = await courseRepository.GetModuleByIdAsync(lesson.ModuleId);
        if (module == null) return false;

        lesson.Title = title;
        lesson.Content = content;
        lesson.VideoUrl = videoUrl;
        lesson.Type = string.IsNullOrEmpty(videoUrl) ? "Text" : "Video";
        lesson.Duration = duration;
        lesson.OrderIndex = orderIndex;

        await courseRepository.UpdateLessonAsync(lesson);
        await courseRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(module.CourseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }

    public async Task<bool> DeleteLessonAsync(Guid lessonId)
    {
        var lesson = await courseRepository.GetLessonByIdAsync(lessonId);
        if (lesson == null) return false;

        var module = await courseRepository.GetModuleByIdAsync(lesson.ModuleId);
        if (module == null) return false;

        await courseRepository.RemoveLessonAsync(lesson);
        await courseRepository.SaveChangesAsync();

        if (broadcaster != null)
        {
            var courseVm = await GetCourseViewModelAsync(module.CourseId);
            if (courseVm != null) await broadcaster.BroadcastCourseUpdatedAsync(courseVm);
        }

        return true;
    }
}
