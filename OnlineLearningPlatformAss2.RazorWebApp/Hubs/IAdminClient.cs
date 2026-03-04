using OnlineLearningPlatformAss2.Service.DTOs.Admin;

namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

public interface IAdminClient
{
    Task ReceiveUserAdded(AdminUserDto user);
    Task ReceiveUserUpdated(AdminUserDto user);
    Task ReceiveUserRoleChanged(Guid userId, string newRole);
    Task ReceiveUserStatusToggled(Guid userId, bool isActive);
    Task ReceiveUserDeleted(Guid userId);
    Task ReceiveUserPasswordReset(Guid userId);
}
