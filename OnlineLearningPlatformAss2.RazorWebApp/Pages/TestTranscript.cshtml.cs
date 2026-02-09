using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages
{
    public class TestTranscriptModel : PageModel
    {
        private readonly ITranscriptService _transcriptService;

        public TestTranscriptModel(ITranscriptService transcriptService)
        {
            _transcriptService = transcriptService;
        }

        [BindProperty]
        public string Transcript { get; set; } = string.Empty;

        [BindProperty]
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task OnPostAsync(string videoUrl, bool summarize)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                ErrorMessage = "Please enter a video URL.";
                return;
            }

            try
            {
                Transcript = await _transcriptService.GenerateTranscriptFromVideoAsync(videoUrl, summarize);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }
}
