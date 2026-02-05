using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatformAss2.RazorWebApp.Hubs;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Services;

/// <summary>
/// Implementation of ICourseUpdateBroadcaster using SignalR IHubContext.
/// Registered in DI container to allow Service layer to trigger real-time updates.
/// </summary>
public class SignalRCourseUpdateBroadcaster : ICourseUpdateBroadcaster
{
    private readonly IHubContext<CourseHub, ICourseClient> _hubContext;

    public SignalRCourseUpdateBroadcaster(IHubContext<CourseHub, ICourseClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastCourseAddedAsync(CourseViewModel course)
    {
        await _hubContext.Clients.All.ReceiveCourseAdded(course);
    }

    public async Task BroadcastCourseUpdatedAsync(CourseViewModel course)
    {
        await _hubContext.Clients.All.ReceiveCourseUpdated(course);
    }

    public async Task BroadcastCourseDeletedAsync(Guid courseId)
    {
        await _hubContext.Clients.All.ReceiveCourseDeleted(courseId);
    }
}
