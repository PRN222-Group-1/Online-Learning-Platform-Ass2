using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using FluentAssertions;
using Xunit;
using Moq;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class AdminServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    private Mock<INotificationService> GetMockNotificationService() => new Mock<INotificationService>();

    #region ApproveCourseAsync Tests

    [Fact]
    public async Task ApproveCourseAsync_ValidCourse_ShouldUpdateStatusToPublished()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var course = new Course { Id = Guid.NewGuid(), Title = "Pending Course", Description = "Test Description", Status = "Pending", InstructorId = Guid.NewGuid() };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ApproveCourseAsync(course.Id);

        // Assert
        result.Should().BeTrue();
        var updatedCourse = await context.Courses.FindAsync(course.Id);
        updatedCourse!.Status.Should().Be("Published");
        updatedCourse.RejectionReason.Should().BeNull();
    }

    [Fact]
    public async Task ApproveCourseAsync_NonExistentCourse_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.ApproveCourseAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveCourseAsync_EmptyGuid_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.ApproveCourseAsync(Guid.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveCourseAsync_ShouldSendNotification()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var instructorId = Guid.NewGuid();
        var course = new Course { Id = Guid.NewGuid(), Title = "Test Course", Description = "Desc", Status = "Pending", InstructorId = instructorId };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        await service.ApproveCourseAsync(course.Id);

        // Assert
        mockNotification.Verify(n => n.SendNotificationAsync(instructorId, It.Is<string>(s => s.Contains("approved")), "Approval"), Times.Once);
    }

    #endregion

    #region RejectCourseAsync Tests

    [Fact]
    public async Task RejectCourseAsync_ValidCourse_ShouldUpdateStatusToRejected()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var course = new Course { Id = Guid.NewGuid(), Title = "Pending Course", Description = "Desc", Status = "Pending", InstructorId = Guid.NewGuid() };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var result = await service.RejectCourseAsync(course.Id, "Content violates guidelines");

        // Assert
        result.Should().BeTrue();
        var updatedCourse = await context.Courses.FindAsync(course.Id);
        updatedCourse!.Status.Should().Be("Rejected");
        updatedCourse.RejectionReason.Should().Be("Content violates guidelines");
    }

    [Fact]
    public async Task RejectCourseAsync_NonExistentCourse_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.RejectCourseAsync(Guid.NewGuid(), "Reason");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RejectCourseAsync_EmptyReason_ShouldStillSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var course = new Course { Id = Guid.NewGuid(), Title = "Course", Description = "Desc", Status = "Pending", InstructorId = Guid.NewGuid() };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var result = await service.RejectCourseAsync(course.Id, "");

        // Assert
        result.Should().BeTrue();
        var updatedCourse = await context.Courses.FindAsync(course.Id);
        updatedCourse!.Status.Should().Be("Rejected");
    }

    #endregion

    #region SuspendCourseAsync Tests

    [Fact]
    public async Task SuspendCourseAsync_PublishedCourse_ShouldSuspend()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var course = new Course { Id = Guid.NewGuid(), Title = "Course", Description = "Desc", Status = "Published", InstructorId = Guid.NewGuid() };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SuspendCourseAsync(course.Id);

        // Assert
        result.Should().BeTrue();
        var updatedCourse = await context.Courses.FindAsync(course.Id);
        updatedCourse!.Status.Should().Be("Suspended");
    }

    [Fact]
    public async Task SuspendCourseAsync_NonExistentCourse_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.SuspendCourseAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UnsuspendCourseAsync Tests

    [Fact]
    public async Task UnsuspendCourseAsync_SuspendedCourse_ShouldUnsuspend()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var course = new Course { Id = Guid.NewGuid(), Title = "Course", Description = "Desc", Status = "Suspended", InstructorId = Guid.NewGuid() };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UnsuspendCourseAsync(course.Id);

        // Assert
        result.Should().BeTrue();
        var updatedCourse = await context.Courses.FindAsync(course.Id);
        updatedCourse!.Status.Should().Be("Published");
    }

    [Fact]
    public async Task UnsuspendCourseAsync_NonExistentCourse_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.UnsuspendCourseAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ToggleUserStatusAsync Tests

    [Fact]
    public async Task ToggleUserStatusAsync_ActiveUser_ShouldDeactivate()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@test.com", PasswordHash = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ToggleUserStatusAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleUserStatusAsync_InactiveUser_ShouldActivate()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@test.com", PasswordHash = "hash", IsActive = false };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ToggleUserStatusAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleUserStatusAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.ToggleUserStatusAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ChangeUserRoleAsync Tests

    [Fact]
    public async Task ChangeUserRoleAsync_ValidRole_ShouldChangeRole()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var studentRole = new Role { Id = Guid.NewGuid(), Name = "Student", Description = "Student" };
        var instructorRole = new Role { Id = Guid.NewGuid(), Name = "Instructor", Description = "Instructor" };
        context.Roles.AddRange(studentRole, instructorRole);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@test.com", PasswordHash = "hash", RoleId = studentRole.Id };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ChangeUserRoleAsync(user.Id, "Instructor");

        // Assert
        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.RoleId.Should().Be(instructorRole.Id);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_NonExistentRole_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ChangeUserRoleAsync(user.Id, "NonExistentRole");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeUserRoleAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var role = new Role { Id = Guid.NewGuid(), Name = "Student", Description = "Student" };
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ChangeUserRoleAsync(Guid.NewGuid(), "Student");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_ValidUser_ShouldDelete()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteUserAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var deletedUser = await context.Users.FindAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task DeleteUserAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.DeleteUserAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_InstructorWithStudents_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var instructorRole = new Role { Id = Guid.NewGuid(), Name = "Instructor", Description = "Instructor" };
        context.Roles.Add(instructorRole);
        var instructor = new User { Id = Guid.NewGuid(), Username = "instructor", Email = "inst@test.com", PasswordHash = "hash", RoleId = instructorRole.Id, Role = instructorRole };
        var course = new Course { Id = Guid.NewGuid(), Title = "Course", Description = "Desc", InstructorId = instructor.Id };
        var student = new User { Id = Guid.NewGuid(), Username = "student", Email = "stu@test.com", PasswordHash = "hash" };
        var enrollment = new Enrollment { Id = Guid.NewGuid(), UserId = student.Id, CourseId = course.Id };
        context.Users.AddRange(instructor, student);
        context.Courses.Add(course);
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteUserAsync(instructor.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddInternalUserAsync Tests

    [Fact]
    public async Task AddInternalUserAsync_ValidData_ShouldCreateUser()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var role = new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Admin" };
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        // Act
        var result = await service.AddInternalUserAsync("newadmin", "admin@test.com", "password123", "Admin");

        // Assert
        result.Should().BeTrue();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == "newadmin");
        user.Should().NotBeNull();
        user!.Email.Should().Be("admin@test.com");
        // Verify password is hashed
        BCrypt.Net.BCrypt.Verify("password123", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task AddInternalUserAsync_NonExistentRole_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.AddInternalUserAsync("newuser", "user@test.com", "password123", "NonExistentRole");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetStatsAsync Tests

    [Fact]
    public async Task GetStatsAsync_ShouldReturnCorrectStats()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var instructorRole = new Role { Id = Guid.NewGuid(), Name = "Instructor", Description = "Instructor" };
        var studentRole = new Role { Id = Guid.NewGuid(), Name = "Student", Description = "Student" };
        context.Roles.AddRange(instructorRole, studentRole);
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), Username = "instructor", Email = "i@test.com", PasswordHash = "hash", RoleId = instructorRole.Id, Role = instructorRole },
            new User { Id = Guid.NewGuid(), Username = "student", Email = "s@test.com", PasswordHash = "hash", RoleId = studentRole.Id, Role = studentRole }
        );
        context.Courses.AddRange(
            new Course { Id = Guid.NewGuid(), Title = "C1", Description = "D", Status = "Published", InstructorId = Guid.NewGuid() },
            new Course { Id = Guid.NewGuid(), Title = "C2", Description = "D", Status = "Pending", InstructorId = Guid.NewGuid() }
        );
        context.Orders.Add(new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TotalAmount = 100, Status = "Completed", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var stats = await service.GetStatsAsync();

        // Assert
        stats.TotalUsers.Should().Be(2);
        stats.TotalInstructors.Should().Be(1);
        stats.TotalCourses.Should().Be(2);
        stats.PendingCourses.Should().Be(1);
        stats.TotalRevenue.Should().Be(100);
        stats.TotalNetProfit.Should().Be(30); // 30% of 100
    }

    [Fact]
    public async Task GetStatsAsync_EmptyDatabase_ShouldReturnZeros()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var stats = await service.GetStatsAsync();

        // Assert
        stats.TotalUsers.Should().Be(0);
        stats.TotalCourses.Should().Be(0);
        stats.TotalRevenue.Should().Be(0);
    }

    #endregion

    #region GetPendingCoursesAsync Tests

    [Fact]
    public async Task GetPendingCoursesAsync_ShouldReturnOnlyPendingCourses()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var cat = new Category { Id = Guid.NewGuid(), Name = "Test", Description = "Test" };
        var instructor = new User { Id = Guid.NewGuid(), Username = "instructor", Email = "i@test.com", PasswordHash = "hash" };
        context.Categories.Add(cat);
        context.Users.Add(instructor);
        context.Courses.AddRange(
            new Course { Id = Guid.NewGuid(), Title = "Pending", Description = "D", Status = "Pending", CategoryId = cat.Id, InstructorId = instructor.Id },
            new Course { Id = Guid.NewGuid(), Title = "Published", Description = "D", Status = "Published", CategoryId = cat.Id, InstructorId = instructor.Id }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPendingCoursesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Pending");
    }

    [Fact]
    public async Task GetPendingCoursesAsync_NoPendingCourses_ShouldReturnEmpty()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);

        // Act
        var result = await service.GetPendingCoursesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllCoursesAsync Tests

    [Fact]
    public async Task GetAllCoursesAsync_ShouldReturnAllCourses()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var cat = new Category { Id = Guid.NewGuid(), Name = "Test", Description = "Test" };
        var instructor = new User { Id = Guid.NewGuid(), Username = "instructor", Email = "i@test.com", PasswordHash = "hash" };
        context.Categories.Add(cat);
        context.Users.Add(instructor);
        context.Courses.AddRange(
            new Course { Id = Guid.NewGuid(), Title = "C1", Description = "D", Status = "Pending", CategoryId = cat.Id, InstructorId = instructor.Id },
            new Course { Id = Guid.NewGuid(), Title = "C2", Description = "D", Status = "Published", CategoryId = cat.Id, InstructorId = instructor.Id },
            new Course { Id = Guid.NewGuid(), Title = "C3", Description = "D", Status = "Rejected", CategoryId = cat.Id, InstructorId = instructor.Id }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllCoursesAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsersWithRoles()
    {
        // Arrange
        using var context = GetDbContext();
        var mockNotification = GetMockNotificationService();
        var service = new AdminService(context, mockNotification.Object);
        var role = new Role { Id = Guid.NewGuid(), Name = "Student", Description = "Student" };
        context.Roles.Add(role);
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), Username = "u1", Email = "u1@test.com", PasswordHash = "hash", RoleId = role.Id, Role = role },
            new User { Id = Guid.NewGuid(), Username = "u2", Email = "u2@test.com", PasswordHash = "hash", RoleId = role.Id, Role = role }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion
}
