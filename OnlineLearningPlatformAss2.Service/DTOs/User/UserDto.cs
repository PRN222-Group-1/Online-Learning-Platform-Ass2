namespace OnlineLearningPlatformAss2.Service.DTOs.User;

public record UserDto(Guid Id, string Username, string Email, string? Role, DateTime CreatedAt);
