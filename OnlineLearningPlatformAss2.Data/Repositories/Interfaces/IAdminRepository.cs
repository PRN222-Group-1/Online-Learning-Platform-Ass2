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
    Task<List<(string Label, int Count)>> GetEnrollmentChartDataAsync(DateTime? startDate, DateTime? endDate);
    Task<List<(string Label, decimal Total)>> GetRevenueChartDataAsync(DateTime? startDate, DateTime? endDate);
    
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

    // Course deletion by ID
    Task DeleteCourseAsync(Guid courseId);
    
    // Users
    Task<bool> HasActiveStudentsAsync(Guid instructorId);
    
    Task<int> SaveChangesAsync();
}
