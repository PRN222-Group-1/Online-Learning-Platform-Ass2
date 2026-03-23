using OnlineLearningPlatformAss2.Data.Entities;

namespace OnlineLearningPlatformAss2.Data.Repositories.Interfaces;

public interface IAdminRepository
{
    // Stats
    Task<int> GetUserCountAsync();
    Task<int> GetInstructorCountAsync();
    Task<int> GetCourseCountAsync();
    Task<int> GetPendingCourseCountAsync();
    Task<int> GetEnrollmentCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int count);
    Task<List<(int Year, int Month, int Count)>> GetMonthlyEnrollmentsAsync(int months);
    Task<List<(int Year, int Month, decimal Total)>> GetMonthlyRevenueAsync(int months);
    
    // Courses
    Task<IEnumerable<Course>> GetPendingCoursesAsync();
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseByIdAsync(Guid courseId);
    Task<Course?> GetCourseWithDetailsAsync(Guid courseId);
    Task AddCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(Course course);
    
    // Categories & Instructors
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<IEnumerable<User>> GetInstructorsAsync();

    // Course CRUD
    Task AddCourseAsync(Course course);
    Task DeleteCourseAsync(Guid courseId);
    
    // Users
    Task<bool> HasActiveStudentsAsync(Guid instructorId);
    
    Task<int> SaveChangesAsync();
}
