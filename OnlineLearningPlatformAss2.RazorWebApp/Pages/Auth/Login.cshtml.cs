using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Account
{
    public class LoginModel(IUserService userService, ILogger<LoginModel> logger) : PageModel
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly ILogger<LoginModel> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Call the UserService to authenticate
                var loginDto = new OnlineLearningPlatformAss2.Service.DTOs.User.UserLoginDto
                {
                    Email = Input.Email,
                    UsernameOrEmail = Input.Email,
                    Password = Input.Password,
                    RememberMe = Input.RememberMe
                };

                var result = await _userService.LoginAsync(loginDto);

                if (result.Success && result.Data != null)
                {
                    // Create Claims-based identity for [Authorize] to work
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, result.Data.UserId.ToString()),
                        new(ClaimTypes.Name, result.Data.Username),
                        new(ClaimTypes.Email, result.Data.Email),
                        new(ClaimTypes.Role, result.Data.RoleName ?? "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = Input.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(Input.RememberMe ? 30 : 1)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Store user info in session
                    HttpContext.Session.SetString("UserId", result.Data.UserId.ToString());
                    HttpContext.Session.SetString("UserEmail", result.Data.Email);
                    HttpContext.Session.SetString("UserName", $"{result.Data.FirstName} {result.Data.LastName}".Trim());
                    HttpContext.Session.SetString("UserRole", result.Data.RoleName);
                    HttpContext.Session.SetString("AvatarUrl", result.Data.AvatarUrl ?? string.Empty);

                    _logger.LogInformation("User {Email} logged in successfully with role {Role}", Input.Email, result.Data.RoleName);
                    SuccessMessage = "Login successful!";

                    // Role-based redirect
                    return result.Data.RoleName?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true
                        ? RedirectToPage("/Admin/Dashboard")
                        : RedirectToPage("/User/Dashboard");
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning("Failed login attempt for {Email}: {Message}", Input.Email, result.Message);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for {Email}", Input.Email);
                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
        }

        public IActionResult OnPostGoogle()
        {
            // TODO: Implement Google Authentication Challenge
            // This would typically involve using HttpContext.ChallengeAsync
            // for the Google authentication scheme.

            ErrorMessage = "Google authentication is not yet configured.";
            return Page();
        }
    }
}
