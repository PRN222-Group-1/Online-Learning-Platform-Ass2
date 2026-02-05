using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Instructor;

[Authorize]
public class BecomeInstructorModel : PageModel
{
    private readonly IUserService _userService;

    public BecomeInstructorModel(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult OnGetAsync()
    {
        // Check if already an instructor
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role?.ToLower() == "instructor")
        {
            return RedirectToPage("/Instructor/Dashboard");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostApplyAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdString, out var userId))
        {
            var success = await _userService.UpgradeToInstructorAsync(userId);
            if (success)
            {
                // Refresh authentication cookie to include active 'Instructor' role immediately
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new(ClaimTypes.Name, user.Username),
                        new(ClaimTypes.Email, user.Email),
                        new(ClaimTypes.Role, "Instructor") // We know they are an instructor now
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                }

                TempData["SuccessMessage"] = "Congratulations! You are now an instructor. Your dashboard is ready!";
                return RedirectToPage("/Instructor/Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Failed to upgrade account. Please contact support.");
            }
        }
        else
        {
            // If userIdString is not a valid GUID, it means the user is not properly authenticated or their ID is malformed.
            // Redirect to login or show an error.
            ModelState.AddModelError("", "User identifier not found or invalid. Please log in again.");
        }
        return Page();
    }
}
