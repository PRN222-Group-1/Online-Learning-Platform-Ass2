using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.User;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.User;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ICourseService _courseService;

    public ProfileModel(IUserService userService, ICourseService courseService)
    {
        _userService = userService;
        _courseService = courseService;
    }

    public UserProfileDto? UserProfile { get; set; }
    public IEnumerable<CourseViewModel> Wishlist { get; set; } = new List<CourseViewModel>();

    [BindProperty]
    public UpdateProfileRequest UpdateRequest { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        UserProfile = await _userService.GetUserProfileAsync(userId);
        
        if (UserProfile == null)
        {
            return NotFound();
        }

        Wishlist = await _courseService.GetWishlistAsync(userId);

        // Pre-populate update request
        UpdateRequest = new UpdateProfileRequest
        {
            FirstName = UserProfile.FirstName,
            LastName = UserProfile.LastName,
            Phone = UserProfile.Phone,
            Address = UserProfile.Address,
            AvatarUrl = UserProfile.AvatarUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        // We'll use the user service to update the profile
        // Assuming IUserService has UpdateProfileAsync or similar
        // For now, let's implement the logic directly if needed or update IUserService
        await _userService.UpdateProfileAsync(userId, UpdateRequest);
        
        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToPage();
    }

    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
