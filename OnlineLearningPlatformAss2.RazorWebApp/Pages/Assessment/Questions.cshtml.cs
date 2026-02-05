using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Assessment;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Assessment;

[Authorize]
public class QuestionsModel : PageModel
{
    private readonly IAssessmentService _assessmentService;
    private readonly IUserService _userService;

    public QuestionsModel(IAssessmentService assessmentService, IUserService userService)
    {
        _assessmentService = assessmentService;
        _userService = userService;
    }

    public List<OnlineLearningPlatformAss2.Service.DTOs.Assessment.AssessmentQuestion> Questions { get; set; } = new();
    
    [BindProperty]
    public Dictionary<Guid, Guid> Answers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdString, out var userId))
        {
            if (await _userService.HasCompletedAssessmentAsync(userId))
            {
                TempData["InfoMessage"] = "You have already completed the skill assessment. You can browse your recommended courses below.";
                return RedirectToPage("/Index");
            }
        }

        Questions = await _assessmentService.GetAssessmentQuestionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        try
        {
            var result = await _assessmentService.SubmitAssessmentAsync(userId, Answers);
            
            if (result.Success)
            {
                // Update user assessment status
                await _userService.UpdateAssessmentStatusAsync(userId, true);
                
                TempData["SuccessMessage"] = "Assessment completed successfully! Here are your personalized recommendations.";
                return RedirectToPage("/Assessment/Results", new { assessmentId = result.Data });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to submit assessment");
                Questions = await _assessmentService.GetAssessmentQuestionsAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            Questions = await _assessmentService.GetAssessmentQuestionsAsync();
            return Page();
        }
    }
}

public class AssessmentQuestion
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!;
    public int OrderIndex { get; set; }
    public List<AssessmentOption> Options { get; set; } = new();
}

public class AssessmentOption
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = null!;
    public string SkillLevel { get; set; } = null!;
    public string Category { get; set; } = null!;
}
