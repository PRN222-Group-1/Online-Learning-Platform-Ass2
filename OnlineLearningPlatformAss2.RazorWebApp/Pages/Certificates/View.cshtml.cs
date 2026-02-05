using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Certificates;

public class ViewModel : PageModel
{
    private readonly OnlineLearningContext _context;

    public ViewModel(OnlineLearningContext context)
    {
        _context = context;
    }

    public string Username { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string InstructorName { get; set; } = "";
    public DateTime IssuedDate { get; set; }
    public Guid CertificateId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        // For simplicity, we search for the certificate by ID
        // In a real app, we'd use a unique verification code
        var cert = await _context.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .ThenInclude(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cert == null) return NotFound();

        Username = cert.User.Username;
        CourseTitle = cert.Course.Title;
        InstructorName = cert.Course.Instructor.Username;
        IssuedDate = cert.IssuedAt;
        CertificateId = cert.Id;
        VerificationCode = cert.Id.ToString().Split('-')[0].ToUpper();

        return Page();
    }

    public string VerificationCode { get; set; } = "";
}
