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

    public async Task<List<(string Label, int Count)>> GetEnrollmentChartDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = context.Enrollments.AsQueryable();
        if (startDate.HasValue) query = query.Where(e => e.EnrolledAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(e => e.EnrolledAt <= endDate.Value);

        var isDaily = startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value).TotalDays <= 31;
        
        if (isDaily)
        {
            var data = await query
                .GroupBy(e => e.EnrolledAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();
            return data.Select(x => (x.Date.ToString("dd/MM/yyyy"), x.Count)).ToList();
        }
        else
        {
            if (!startDate.HasValue)
            {
                var minDate = DateTime.UtcNow.AddMonths(-23);
                query = query.Where(e => e.EnrolledAt >= new DateTime(minDate.Year, minDate.Month, 1));
            }
            var data = await query
                .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
            return data.Select(x => ($"{new DateTime(x.Year, x.Month, 1):MMM yyyy}", x.Count)).ToList();
        }
    }

    public async Task<List<(string Label, decimal Total)>> GetRevenueChartDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = context.Orders.Where(o => o.Status == "Completed");
        if (startDate.HasValue) query = query.Where(o => o.CreatedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(o => o.CreatedAt <= endDate.Value);

        var isDaily = startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value).TotalDays <= 31;

        if (isDaily)
        {
            var data = await query
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();
            return data.Select(x => (x.Date.ToString("dd/MM/yyyy"), x.Total)).ToList();
        }
        else
        {
            if (!startDate.HasValue)
            {
                var minDate = DateTime.UtcNow.AddMonths(-23);
                query = query.Where(o => o.CreatedAt >= new DateTime(minDate.Year, minDate.Month, 1));
            }
            var data = await query
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
            return data.Select(x => ($"{new DateTime(x.Year, x.Month, 1):MMM yyyy}", x.Total)).ToList();
        }
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

    // Categories & Instructors
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetInstructorsAsync()
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Role != null && u.Role.Name == "Instructor")
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    // Course CRUD
    public async Task DeleteCourseAsync(Guid courseId)
    {
        var course = await context.Courses.FindAsync(courseId);
        if (course != null)
        {
            context.Courses.Remove(course);
        }
    }

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}
