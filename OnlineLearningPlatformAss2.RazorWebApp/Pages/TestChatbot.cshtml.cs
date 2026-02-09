using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Chatbot;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages
{
    public class TestChatbotModel : PageModel
    {
        private readonly IChatbotService _chatbotService;

        public TestChatbotModel(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [BindProperty]
        public string Question { get; set; } = string.Empty;

        [BindProperty]
        public List<ChatHistoryItem> History { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            if (string.IsNullOrEmpty(Question))
            {
                return;
            }

            try
            {
                // Add user question to history for display (optional, but good for UX)
                // Actually, the service appends current question to history.
                // We should add it to our local history to preserve it.
                
                var answer = await _chatbotService.AskAsync(Question, History);

                History.Add(new ChatHistoryItem { Role = "user", Content = Question });
                History.Add(new ChatHistoryItem { Role = "assistant", Content = answer });

                Question = string.Empty; // Clear input
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }
}
