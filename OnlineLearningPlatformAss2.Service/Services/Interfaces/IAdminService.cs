using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.DTOs.Course;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<CourseViewModel>> GetPendingCoursesAsync();
    Task<bool> ApproveCourseAsync(Guid courseId);
    Task<bool> RejectCourseAsync(Guid courseId, string reason);
    Task<bool> SuspendCourseAsync(Guid courseId);
    Task<bool> UnsuspendCourseAsync(Guid courseId);
    Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync();
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string searchTerm = null);
    Task<bool> ToggleUserStatusAsync(Guid userId);
    Task<bool> ChangeUserRoleAsync(Guid userId, string roleName);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> AddInternalUserAsync(string username, string email, string password, string role);
    Task<bool> ResetUserPasswordAsync(Guid userId, string newPassword);
    
    // Course CRUD
    Task<(IEnumerable<CategoryDto> Categories, IEnumerable<InstructorDto> Instructors)> GetFormDataAsync();
    Task<CourseViewModel?> CreateCourseAsync(AdminCreateCourseDto dto);
    Task<CourseViewModel?> UpdateCourseAsync(AdminUpdateCourseDto dto);
    Task<bool> DeleteCourseAsync(Guid courseId);
}
