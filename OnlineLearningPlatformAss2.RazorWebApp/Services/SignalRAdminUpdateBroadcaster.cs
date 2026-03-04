using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatformAss2.RazorWebApp.Hubs;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Services;

public class SignalRAdminUpdateBroadcaster : IAdminUpdateBroadcaster
{
    private readonly IHubContext<AdminHub, IAdminClient> _hubContext;

    public SignalRAdminUpdateBroadcaster(IHubContext<AdminHub, IAdminClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastUserAddedAsync(AdminUserDto user)
    {
        await _hubContext.Clients.All.ReceiveUserAdded(user);
    }

    public async Task BroadcastUserUpdatedAsync(AdminUserDto user)
    {
        await _hubContext.Clients.All.ReceiveUserUpdated(user);
    }

    public async Task BroadcastUserRoleChangedAsync(Guid userId, string newRole)
    {
        await _hubContext.Clients.All.ReceiveUserRoleChanged(userId, newRole);
    }

    public async Task BroadcastUserStatusToggledAsync(Guid userId, bool isActive)
    {
        await _hubContext.Clients.All.ReceiveUserStatusToggled(userId, isActive);
    }

    public async Task BroadcastUserDeletedAsync(Guid userId)
    {
        await _hubContext.Clients.All.ReceiveUserDeleted(userId);
    }

    public async Task BroadcastUserPasswordResetAsync(Guid userId)
    {
        await _hubContext.Clients.All.ReceiveUserPasswordReset(userId);
    }
}
