namespace OnlineLearningPlatformAss2.Service.DTOs.User;

public record UserLoginResponseDto(Guid Id, string Username, string Email, string? Role, DateTime CreatedAt);
