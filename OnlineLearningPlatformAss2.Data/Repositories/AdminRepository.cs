using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Entities;
using OnlineLearningPlatformAss2.Data.Repositories.Interfaces;

namespace OnlineLearningPlatformAss2.Data.Repositories;

public class AdminRepository(OnlineLearningSystemDbContext context) : IAdminRepository
{
    // Stats
    public async Task<int> GetUserCountAsync()
    {
        return await context.Users.CountAsync();
    }

    public async Task<int> GetInstructorCountAsync()
    {
        return await context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Instructor");
    }

    public async Task<int> GetCourseCountAsync()
    {
        return await context.Courses.CountAsync();
    }

    public async Task<int> GetPendingCourseCountAsync()
    {
        return await context.Courses.CountAsync(c => c.Status == "Pending");
    }

    public async Task<int> GetEnrollmentCountAsync()
    {
        return await context.Enrollments.CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await context.Orders.Where(o => o.Status == "Completed").SumAsync(o => o.TotalAmount);
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Course)
            .Include(o => o.Path)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    // Courses
    public async Task<IEnumerable<Course>> GetPendingCoursesAsync()
    {
        return await context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Where(c => c.Status == "Pending")
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .ToListAsync();
    }

    public async Task<Course?> GetCourseByIdAsync(Guid courseId)
    {
        return await context.Courses.FindAsync(courseId);
    }

    public async Task<Course?> GetCourseWithDetailsAsync(Guid courseId)
    {
        return await context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
    }

    public async Task AddCourseAsync(Course course)
    {
        await context.Courses.AddAsync(course);
    }

    public async Task UpdateCourseAsync(Course course)
    {
        context.Courses.Update(course);
        await Task.CompletedTask;
    }

    public async Task DeleteCourseAsync(Course course)
    {
        context.Courses.Remove(course);
        await Task.CompletedTask;
    }

    // Users
    public async Task<bool> HasActiveStudentsAsync(Guid instructorId)
    {
        return await context.Enrollments.AnyAsync(e => e.Course.InstructorId == instructorId);
    }

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}
