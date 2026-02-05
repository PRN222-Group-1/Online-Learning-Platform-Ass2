using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatformAss2.Service.DTOs.Course;

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

    public async Task JoinCourseListGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, CourseListGroup);
    }

    public async Task NotifyCourseAdded(CourseViewModel course)
    {
        await Clients.Group(CourseListGroup).ReceiveCourseAdded(course);
    }

    public async Task NotifyCourseUpdated(CourseViewModel course)
    {
        await Clients.Group(CourseListGroup).ReceiveCourseUpdated(course);
    }

    public async Task NotifyCourseDeleted(Guid courseId)
    {
        await Clients.Group(CourseListGroup).ReceiveCourseDeleted(courseId);
    }
}
