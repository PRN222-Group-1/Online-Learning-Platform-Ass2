using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

[Authorize(Roles = "Admin")]
public class AdminHub : Hub<IAdminClient>
{
    private readonly IAdminService _adminService;

    public AdminHub(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task AddUser(string username, string email, string password, string role)
    {
        var success = await _adminService.AddInternalUserAsync(username, email, password, role);
        if (!success)
        {
            throw new HubException("Failed to add user.");
        }
    }

    public async Task ToggleUserStatus(Guid userId)
    {
        var success = await _adminService.ToggleUserStatusAsync(userId);
        if (!success)
        {
            throw new HubException("Failed to toggle user status.");
        }
    }

    public async Task ChangeUserRole(Guid userId, string role)
    {
        var success = await _adminService.ChangeUserRoleAsync(userId, role);
        if (!success)
        {
            throw new HubException("Failed to change user role.");
        }
    }

    public async Task DeleteUser(Guid userId)
    {
        var success = await _adminService.DeleteUserAsync(userId);
        if (!success)
        {
            throw new HubException("Failed to delete user. Make sure it's not the main admin or an instructor with students.");
        }
    }

    public async Task ResetUserPassword(Guid userId, string newPassword)
    {
        var success = await _adminService.ResetUserPasswordAsync(userId, newPassword);
        if (!success)
        {
            throw new HubException("Failed to reset user password.");
        }
    }
}
