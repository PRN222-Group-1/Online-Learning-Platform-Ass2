namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<Guid>> RegisterAsync(UserRegisterDto dto);
    Task<ServiceResult<UserLoginResponseDto>> LoginAsync(UserLoginDto dto);
}
