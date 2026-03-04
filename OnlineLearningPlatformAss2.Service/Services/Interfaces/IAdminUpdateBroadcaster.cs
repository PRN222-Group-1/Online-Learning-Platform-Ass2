using OnlineLearningPlatformAss2.Service.DTOs.Admin;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IAdminUpdateBroadcaster
{
    Task BroadcastUserAddedAsync(AdminUserDto user);
    Task BroadcastUserUpdatedAsync(AdminUserDto user);
    Task BroadcastUserRoleChangedAsync(Guid userId, string newRole);
    Task BroadcastUserStatusToggledAsync(Guid userId, bool isActive);
    Task BroadcastUserDeletedAsync(Guid userId);
    Task BroadcastUserPasswordResetAsync(Guid userId);
}
