using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Data.Repositories.Interfaces;
using OnlineLearningPlatformAss2.Service.Common;
using OnlineLearningPlatformAss2.Service.DTOs.User;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.Utils;

namespace OnlineLearningPlatformAss2.Service.Services.Implementations;

public class UserService(IBaseRepository<User> userRepository, IBaseRepository<Profile> profileRepository) : IUserService
{
    private readonly IBaseRepository<User> _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IBaseRepository<Profile> _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));

    /// <summary>
    /// Registers a new user in the system
    /// </summary>
    public async Task<ServiceResult<Guid>> RegisterAsync(UserRegisterDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email))
            return ServiceResult<Guid>.FailureResult("Email is required");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return ServiceResult<Guid>.FailureResult("Password is required");

        if (dto.Password != dto.ConfirmPassword)
            return ServiceResult<Guid>.FailureResult("Password and confirm password do not match");

        if (dto.Password.Length < 6)
            return ServiceResult<Guid>.FailureResult("Password must be at least 6 characters long");

        // Check if user already exists
        var existingUsers = await _userRepository.GetAllAsync();
        if (existingUsers.Any(u => u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
            return ServiceResult<Guid>.FailureResult("Email is already registered");

        try
        {
            // Hash the password
            var hashedPassword = PasswordUtils.HashPassword(dto.Password);

            // Create new user (assign to Student role - guid: 00000000-0000-0000-0000-000000000003)
            var user = new User
            {
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = hashedPassword,
                RoleId = Guid.Parse("00000000-0000-0000-0000-000000000003"), // Student role
                IsDeleted = false
            };

            // Add user to database
            var createdUser = await _userRepository.AddAsync(user);

            // Create a profile for the user
            var profile = new Profile
            {
                UserId = createdUser.Id,
                FirstName = string.Empty,
                LastName = string.Empty,
                AvatarUrl = string.Empty,
                Description = string.Empty
            };

            await _profileRepository.AddAsync(profile);

            Log.Information("User registered successfully: {Email}", dto.Email);
            return ServiceResult<Guid>.SuccessResult(createdUser.Id, "User registered successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering user: {Email}", dto.Email);
            return ServiceResult<Guid>.FailureResult("An error occurred while registering the user");
        }
    }

    /// <summary>
    /// Authenticates a user and returns user details
    /// </summary>
    public async Task<ServiceResult<UserLoginResponseDto>> LoginAsync(UserLoginDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email))
            return ServiceResult<UserLoginResponseDto>.FailureResult("Email is required");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return ServiceResult<UserLoginResponseDto>.FailureResult("Password is required");

        try
        {
            // Find user by email
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email.Equals(dto.Email.ToLower().Trim(), StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                Log.Warning("Login attempt with non-existent email: {Email}", dto.Email);
                return ServiceResult<UserLoginResponseDto>.FailureResult("Invalid email or password");
            }

            // Verify password
            if (!PasswordUtils.VerifyPassword(dto.Password, user.PasswordHash))
            {
                Log.Warning("Failed login attempt for user: {Email}", dto.Email);
                return ServiceResult<UserLoginResponseDto>.FailureResult("Invalid email or password");
            }

            // Get user profile
            var profiles = await _profileRepository.GetAllAsync();
            var profile = profiles.FirstOrDefault(p => p.UserId == user.Id);

            if (profile is null)
            {
                Log.Warning("User profile not found for user: {UserId}", user.Id);
                return ServiceResult<UserLoginResponseDto>.FailureResult("User profile not found");
            }

            // Get role name
            var roleName = user.Role?.Name ?? "Unknown";

            var response = new UserLoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                AvatarUrl = profile.AvatarUrl,
                RoleName = roleName
            };

            Log.Information("User logged in successfully: {Email}", dto.Email);
            return ServiceResult<UserLoginResponseDto>.SuccessResult(response, "Login successful");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during login for email: {Email}", dto.Email);
            return ServiceResult<UserLoginResponseDto>.FailureResult("An error occurred during login");
        }
    }
}
