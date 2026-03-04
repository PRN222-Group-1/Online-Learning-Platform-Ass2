using Microsoft.AspNetCore.SignalR;

namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

public class CourseHub : Hub<ICourseClient>
{
    private const string CourseListGroup = "CourseListViewers";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, CourseListGroup);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, CourseListGroup);
        await base.OnDisconnectedAsync(exception);
    }
}
