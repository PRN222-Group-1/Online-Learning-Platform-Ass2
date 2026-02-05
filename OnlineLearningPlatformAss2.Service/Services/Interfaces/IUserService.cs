using OnlineLearningPlatformAss2.Service.DTOs.User;
using OnlineLearningPlatformAss2.Service.Results;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<Guid>> RegisterAsync(UserRegisterDto dto);
    Task<ServiceResult<UserLoginResponseDto>> LoginAsync(UserLoginDto dto);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> HasCompletedAssessmentAsync(Guid userId);
    Task UpdateAssessmentStatusAsync(Guid userId, bool completed);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, dynamic updateRequest);
    Task<bool> UpgradeToInstructorAsync(Guid userId);
    Task<bool> UserExistsAsync(string usernameOrEmail);
}
