namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.User;

public class RegisterModel : PageModel
{
    private readonly IUserService _userService;

    public RegisterModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public UserRegisterDto RegisterDto { get; set; } = new("", "", "", "");

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _userService.RegisterAsync(RegisterDto);

        if (result.Success && result.Data != Guid.Empty)
        {
            // Auto-login the newly registered user
            var loginDto = new UserLoginDto(
                RegisterDto.Username,
                RegisterDto.Password
            );
            
            var loginResult = await _userService.LoginAsync(loginDto);
            
            if (loginResult.Success && loginResult.Data is not null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, loginResult.Data.Id.ToString()),
                    new(ClaimTypes.Name, loginResult.Data.Username),
                    new(ClaimTypes.Email, loginResult.Data.Email),
                    new(ClaimTypes.Role, loginResult.Data.Role ?? "User")
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

                TempData["WelcomeMessage"] = "Welcome! Let's personalize your learning journey.";
                return RedirectToPage("/Assessment/Start");
            }
        }

        ModelState.AddModelError(string.Empty, result.Message ?? "Registration failed");
        return Page();
    }
}
