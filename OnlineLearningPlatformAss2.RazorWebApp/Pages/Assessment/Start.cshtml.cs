using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Assessment;

[Authorize]
public class StartModel : PageModel
{
    private readonly IUserService _userService;

    public StartModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
        {
            var hasCompleted = await _userService.HasCompletedAssessmentAsync(userId);
            if (hasCompleted)
            {
                // Allow unlimited retakes - encourage users to update their preferences
                TempData["InfoMessage"] = "Welcome back! Retake the assessment anytime to get updated course recommendations.";
            }
        }
    }
}
