using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Assessment;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Assessment;

[Authorize]
public class ResultsModel : PageModel
{
    private readonly IAssessmentService _assessmentService;

    public ResultsModel(IAssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    public OnlineLearningPlatformAss2.Service.DTOs.Assessment.AssessmentResult? Assessment { get; set; }
    public List<OnlineLearningPlatformAss2.Service.DTOs.Assessment.CourseRecommendation> Recommendations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid assessmentId)
    {
        // Get assessment results
        Assessment = await _assessmentService.GetAssessmentResultAsync(assessmentId);
        
        if (Assessment == null)
        {
            TempData["ErrorMessage"] = "Assessment results not found.";
            return RedirectToPage("/Assessment/Start");
        }

        // Get course recommendations
        Recommendations = await _assessmentService.GetRecommendationsAsync(Assessment.UserId);

        return Page();
    }
}
