using OnlineLearningPlatformAss2.Service.DTOs.Chatbot;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IChatbotService
{
    Task<string> AskAsync(string question, List<ChatHistoryItem> history);
    Task<string> SummarizeTranscriptAsync(string transcript);
    Task<string> AskWithContextAsync(string question, string context);
}
