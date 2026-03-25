using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.User;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.User;

public class LoginModel : PageModel
{
    private readonly IUserService _userService;

    public LoginModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public UserLoginDto LoginDto { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _userService.LoginAsync(LoginDto);

        if (result.Success && result.Data is not null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Data.Id.ToString()),
                new(ClaimTypes.Name, result.Data.Username),
                new(ClaimTypes.Email, result.Data.Email),
                new(ClaimTypes.Role, result.Data.Role ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Store user info in session for easy access
            HttpContext.Session.SetString("UserId", result.Data.Id.ToString());
            HttpContext.Session.SetString("UserEmail", result.Data.Email);
            HttpContext.Session.SetString("UserName", result.Data.Username);
            HttpContext.Session.SetString("UserRole", result.Data.Role ?? "User");
            if (!string.IsNullOrEmpty(result.Data.AvatarUrl))
            {
                HttpContext.Session.SetString("AvatarUrl", result.Data.AvatarUrl);
            }

            SuccessMessage = "Login successful!";
            
            // Role-based redirect
            if (result.Data.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToPage("/Admin/Dashboard");
            }
            if (result.Data.Role?.Equals("Instructor", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToPage("/Instructor/Dashboard");
            }

            return RedirectToPage("/Index");
        }

        ErrorMessage = result.Message ?? "Login failed";
        ModelState.AddModelError(string.Empty, ErrorMessage);
        return Page();
    }
}
