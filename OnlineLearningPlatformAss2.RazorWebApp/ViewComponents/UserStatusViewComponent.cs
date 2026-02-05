using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.ViewComponents;

public class UserStatusViewComponent : ViewComponent
{
    private readonly IUserService _userService;

    public UserStatusViewComponent(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userIdString = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
        bool hasCompletedAssessment = false;
        
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
        {
            hasCompletedAssessment = await _userService.HasCompletedAssessmentAsync(userId);
        }

        return View(hasCompletedAssessment);
    }
}
