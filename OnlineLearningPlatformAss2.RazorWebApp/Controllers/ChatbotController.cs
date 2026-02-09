using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatformAss2.Service.DTOs.Chatbot;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatbotRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Question))
        {
            return BadRequest("Question is required.");
        }

        try
        {
            var answer = await _chatbotService.AskAsync(request.Question, request.History);
            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class ChatbotRequest
{
    public string Question { get; set; } = string.Empty;
    public List<ChatHistoryItem> History { get; set; } = new();
}
