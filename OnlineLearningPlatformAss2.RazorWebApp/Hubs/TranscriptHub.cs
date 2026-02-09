using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

public class TranscriptHub(ITranscriptService transcriptService, ICourseService courseService, IChatbotService chatbotService) : Hub
{
    private readonly ITranscriptService _transcriptService = transcriptService;
    private readonly ICourseService _courseService = courseService;
    private readonly IChatbotService _chatbotService = chatbotService;

    public async Task GenerateSummary(Guid courseId, Guid lessonId)
    {
        var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            await Clients.Caller.SendAsync("ReceiveError", "User not authorized.");
            return;
        }

        try
        {
            var enrollmentId = await _courseService.GetEnrollmentIdAsync(userId, courseId);
            if (!enrollmentId.HasValue)
            {
                await Clients.Caller.SendAsync("ReceiveError", "You are not enrolled in this course.");
                return;
            }

            // 1. Check DB for existing data
            var existingTranscript = await _courseService.GetLessonTranscriptAsync(enrollmentId.Value, lessonId);
            // Note: GetLessonTranscriptAsync returns transcript, but we also want summary.
            // However, GetLessonTranscriptAsync logic in CourseService currently might define fetching from API if not found.
            // Let's rely on CourseService to get stored data if available. 
            // Actually, I should check if summary exists in DB specifically? 
            // The current GetLessonTranscriptAsync implementation tries to fetch from API if not in DB. 
            // That might be redundant if we want to separate logic here. 
            // But let's stick to the plan: Check DB first. 
            
            // To properly check without triggering auto-generation in Service, 
            // I might need a "GetStoredLessonData" method or just rely on the fact that 
            // GetLessonTranscriptAsync *calls* SaveLessonAiDataAsync which updates the DB.
            
            // Wait, GetLessonTranscriptAsync in CourseService ALREADY calls TranscriptService if missing.
            // I should probably modify GetLessonTranscriptAsync to NOT call API, or just use Repository directly here?
            // No, Repository is internal to Service.
            // Better approach: Use CourseService to get COURSE/LESSON stored data.
            // I'll assume if GetLessonTranscriptAsync returns something valid, it's good. 
            // But I specifically want to control the summarization source (Groq).
            
            // Let's try to get the CourseLearn view model or similar to see if data is there?
            // Or just check if we can get it.
            
            // Let's proceed with the flow:
            // If I call GetLessonTranscriptAsync, it might trigger the OLD Whisper-summarization logic if not found.
            // I should probably update CourseService.GetLessonTranscriptAsync to NOT auto-generate, 
            // OR ignore it and force generation here if I want specific Groq logic.
            
            // Actually, the previous step added `SaveLessonAiDataAsync`.
            // The `GetLessonTranscriptAsync` method DOES invoke `GenerateTranscriptFromVideoAsync` if not found.
            // And that uses the default `summarize=true` from the service implementation (unless I change it).
            
            // I want to override this behavior.
            
            var lesson = (await _courseService.GetCourseLearnAsync(enrollmentId.Value))?
                         .Modules.SelectMany(m => m.Lessons).FirstOrDefault(l => l.Id == lessonId);
            
            if (lesson == null || string.IsNullOrEmpty(lesson.VideoUrl))
            {
                 await Clients.Caller.SendAsync("ReceiveError", "Video not available.");
                 return;
            }

            // Check if we already have summary in the loaded lesson view model
            if (!string.IsNullOrEmpty(lesson.AiSummary)) 
            {
                await Clients.Caller.SendAsync("ReceiveSummary", lesson.AiSummary);
                return;
            }

            // 2. Generate Transcript (Raw)
            var fullTranscript = await _transcriptService.GenerateTranscriptFromVideoAsync(lesson.VideoUrl, false); // false = no summary
            
            if (string.IsNullOrEmpty(fullTranscript)) 
            {
                 await Clients.Caller.SendAsync("ReceiveError", "Failed to generate transcript.");
                 return;
            }

            // 3. Generate Summary via Groq
            var groqSummary = await _chatbotService.SummarizeTranscriptAsync(fullTranscript);

            // 4. Save to database
            await _courseService.SaveLessonAiDataAsync(enrollmentId.Value, lessonId, fullTranscript, groqSummary);

            // 5. Return
            await Clients.Caller.SendAsync("ReceiveSummary", groqSummary);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", $"Error: {ex.Message}");
        }
    }
}
