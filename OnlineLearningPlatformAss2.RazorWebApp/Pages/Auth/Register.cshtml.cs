using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Account
{
  public class RegisterModel(IUserService userService, ILogger<RegisterModel> logger) : PageModel
  {
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly ILogger<RegisterModel> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
      [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
      [DataType(DataType.Password)]
      public string Password { get; set; } = string.Empty;

      [DataType(DataType.Password)]
      [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
      public string ConfirmPassword { get; set; } = string.Empty;

      [Required(ErrorMessage = "First name is required")]
      [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters")]
      public string FirstName { get; set; } = string.Empty;

      [Required(ErrorMessage = "Last name is required")]
      [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters")]
      public string LastName { get; set; } = string.Empty;

      [Required(ErrorMessage = "You must agree to the terms and conditions")]
      public bool AgreeToTerms { get; set; }
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

      // Validate agreement
      if (!Input.AgreeToTerms)
      {
        ModelState.AddModelError("Input.AgreeToTerms", "You must agree to the terms and conditions");
        return Page();
      }

      try
      {
        // Call the UserService to register
        var registerDto = new OnlineLearningPlatformAss2.Service.DTOs.User.UserRegisterDto
        {
          Email = Input.Email,
          Password = Input.Password,
          ConfirmPassword = Input.ConfirmPassword
        };

        var result = await _userService.RegisterAsync(registerDto);

        if (result.Success)
        {
          // Update the newly created profile with first and last name
          _logger.LogInformation("User {Email} registered successfully. User ID: {UserId}", Input.Email, result.Data);

          SuccessMessage = $"Registration successful! Welcome, {Input.FirstName}. You can now log in with your credentials.";

          // Redirect to login page after a brief moment
          return RedirectToPage("Login", new { email = Input.Email });
        }
        else
        {
          ErrorMessage = result.Message;
          _logger.LogWarning("Failed registration attempt for {Email}: {Message}", Input.Email, result.Message);
          return Page();
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred during registration for {Email}", Input.Email);
        ErrorMessage = "An error occurred during registration. Please try again.";
        return Page();
      }
    }
  }
}
