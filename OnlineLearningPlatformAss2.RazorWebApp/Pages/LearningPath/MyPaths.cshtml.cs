using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.LearningPath;

[Authorize]
public class MyPathsModel : PageModel
{
    private readonly ILearningPathService _learningPathService;

    public MyPathsModel(ILearningPathService learningPathService)
    {
        _learningPathService = learningPathService;
    }

    public List<UserLearningPathWithProgressDto> EnrolledPaths { get; set; } = new();
    public bool HasPaths { get; set; }

    public async Task OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
        {
            var enrollments = await _learningPathService.GetUserEnrolledPathsAsync(userId);
            EnrolledPaths = enrollments.ToList();
            HasPaths = EnrolledPaths.Any();
        }
    }
}
