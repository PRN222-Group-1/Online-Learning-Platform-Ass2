using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.User;
using OnlineLearningPlatformAss2.Service.Results;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace OnlineLearningPlatformAss2.Service.Services;

public class UserService : IUserService
{
    private readonly OnlineLearningContext _context;

    public UserService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<Guid>> RegisterAsync(UserRegisterDto dto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Username))
                return ServiceResult<Guid>.FailureResult("Username is required.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return ServiceResult<Guid>.FailureResult("Email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return ServiceResult<Guid>.FailureResult("Password is required.");

            if (dto.Password != dto.ConfirmPassword)
                return ServiceResult<Guid>.FailureResult("Passwords do not match.");

            if (dto.Password.Length < 6)
                return ServiceResult<Guid>.FailureResult("Password must be at least 6 characters long.");

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower() || 
                                         u.Email.ToLower() == dto.Email.ToLower());
            
            if (existingUser != null)
            {
                if (existingUser.Username.ToLower() == dto.Username.ToLower())
                    return ServiceResult<Guid>.FailureResult("Username already exists.");
                
                if (existingUser.Email.ToLower() == dto.Email.ToLower())
                    return ServiceResult<Guid>.FailureResult("Email already exists.");
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Get default user role
            var defaultRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == "user");

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = hashedPassword,
                CreateAt = DateTime.UtcNow,
                RoleId = defaultRole?.Id,
                HasCompletedAssessment = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return ServiceResult<Guid>.SuccessResult(user.Id);
        }
        catch (Exception ex)
        {
            return ServiceResult<Guid>.FailureResult($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserLoginResponseDto>> LoginAsync(UserLoginDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail))
                return ServiceResult<UserLoginResponseDto>.FailureResult("Username or email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return ServiceResult<UserLoginResponseDto>.FailureResult("Password is required.");

            // Find user by username or email
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => 
                    u.Username.ToLower() == dto.UsernameOrEmail.ToLower() || 
                    u.Email.ToLower() == dto.UsernameOrEmail.ToLower());

            if (user == null)
                return ServiceResult<UserLoginResponseDto>.FailureResult("Invalid username/email or password.");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return ServiceResult<UserLoginResponseDto>.FailureResult("Invalid username/email or password.");

            // Create response DTO
            var response = new UserLoginResponseDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role?.Name,
                user.CreateAt
            );

            return ServiceResult<UserLoginResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserLoginResponseDto>.FailureResult($"Login failed: {ex.Message}");
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDto(
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role != null ? u.Role.Name : null,
                    u.CreateAt
                ))
                .ToListAsync();

            return users;
        }
        catch
        {
            return Enumerable.Empty<UserDto>();
        }
    }

    public async Task<bool> HasCompletedAssessmentAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.HasCompletedAssessment ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task UpdateAssessmentStatusAsync(Guid userId, bool completed)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                user.HasCompletedAssessment = completed;
                if (completed)
                {
                    user.AssessmentCompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Log error if needed
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            return new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role?.Name,
                user.CreateAt
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var enrolledCourses = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Category)
                .Include(e => e.Course.Instructor)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var recentCourses = enrolledCourses.Take(3).Select(e => new CourseViewModel
            {
                Id = e.Course.Id,
                Title = e.Course.Title,
                Description = e.Course.Description,
                Price = e.Course.Price,
                ImageUrl = e.Course.ImageUrl,
                CategoryName = e.Course.Category.Name,
                InstructorName = e.Course.Instructor.Username,
                Rating = 0m, // Placeholder
                EnrollmentDate = e.EnrolledAt,
                Progress = 0 // Will implement progress calculation later
            }).ToList();

            return new UserProfileDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role?.Name,
                user.CreateAt,
                user.Profile?.FirstName,
                user.Profile?.LastName,
                user.Profile?.Phone,
                user.Profile?.Address,
                user.Profile?.DateOfBirth,
                user.Profile?.AvatarUrl,
                enrolledCourses.Count,
                enrolledCourses.Count(e => e.Status == "Completed"),
                enrolledCourses.Any() ? 0 : 0, // Placeholder for overall progress
                user.HasCompletedAssessment,
                recentCourses
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UserExistsAsync(string usernameOrEmail)
    {
        try
        {
            return await _context.Users
                .AnyAsync(u => u.Username.ToLower() == usernameOrEmail.ToLower() || 
                              u.Email.ToLower() == usernameOrEmail.ToLower());
        }
        catch
        {
            return false;
        }
    }

    public async Task UpdateProfileAsync(Guid userId, dynamic updateRequest)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        if (user.Profile == null)
        {
            user.Profile = new Profile { Id = Guid.NewGuid(), UserId = userId };
            _context.Profiles.Add(user.Profile);
        }

        user.Profile.FirstName = updateRequest.FirstName;
        user.Profile.LastName = updateRequest.LastName;
        user.Profile.Phone = updateRequest.Phone;
        user.Profile.Address = updateRequest.Address;
        user.Profile.AvatarUrl = updateRequest.AvatarUrl;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpgradeToInstructorAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        var instructorRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "instructor");

        if (instructorRole == null) return false;

        user.RoleId = instructorRole.Id;
        await _context.SaveChangesAsync();
        return true;
    }
}
