using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                // TODO: Implement actual authentication logic here
                // For demonstration, we just redirect to Index if valid
                return RedirectToPage("/Index");
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public IActionResult OnPostGoogle()
        {
            // TODO: Implement Google Authentication Challenge
            // This would typically involve using HttpContext.ChallengeAsync
            // for the Google authentication scheme.
            
            // For now, redirect to Index as a placeholder or stay on page with a message
            return RedirectToPage("/Index");
        }
    }
}
