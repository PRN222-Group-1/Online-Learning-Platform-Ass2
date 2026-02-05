using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.LearningPath;

public class BrowseModel : PageModel
{
    private readonly ILearningPathService _learningPathService;

    public BrowseModel(ILearningPathService learningPathService)
    {
        _learningPathService = learningPathService;
    }

    public List<LearningPathViewModel> LearningPaths { get; set; } = new();
    public List<LearningPathViewModel> FeaturedPaths { get; set; } = new();
    public bool IsAuthenticated { get; set; }

    public async Task OnGetAsync()
    {
        IsAuthenticated = User.Identity?.IsAuthenticated == true;
        Guid? userId = null;

        if (IsAuthenticated)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out var id))
            {
                userId = id;
            }
        }

        // Get all published learning paths
        var allPaths = await _learningPathService.GetPublishedPathsAsync(userId);
        LearningPaths = allPaths.ToList();

        // Get featured paths
        var featured = await _learningPathService.GetFeaturedLearningPathsAsync(userId);
        FeaturedPaths = featured.ToList();
    }
}
