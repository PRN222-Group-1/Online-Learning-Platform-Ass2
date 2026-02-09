using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OnlineLearningPlatformAss2.Data.Repositories.Interfaces;
using OnlineLearningPlatformAss2.Data.Entities;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Chatbot;

namespace OnlineLearningPlatformAss2.Service.Services;

public class ChatbotService(HttpClient httpClient, ICourseRepository courseRepository, IConfiguration configuration) : IChatbotService
{
    private readonly HttpClient _http = httpClient;
    private readonly ICourseRepository _courseRepository = courseRepository;
    private const string _aiEndpoint = "https://api.groq.com/openai/v1/chat/completions";
    private readonly string _groqApiKey = configuration["GroqAPIKey:Key"] ?? "";

    public async Task<string> AskAsync(string question, List<ChatHistoryItem> history)
    {
        if (string.IsNullOrEmpty(_groqApiKey))
        {
            return "Xin lỗi, chức năng AI chưa được cấu hình (Thiếu API Key).";
        }

        var courses = await _courseRepository.GetCoursesAsync();
        var courseList = courses.ToList();
        var totalCourses = courseList.Count;
        var courseData = string.Join("\n", courseList.Select(c => $"- [ID: {c.CourseId}] {c.Title} (Price: {c.Price:C}): {c.Description}"));

        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = "Bạn là một tư vấn viên khóa học chuyên nghiệp cho nền tảng học trực tuyến. " +
                          $"Hiện tại hệ thống đang có tổng cộng {totalCourses} khóa học. " +
                          "Dưới đây là danh sách chi tiết các khóa học:\n" +
                          courseData + "\n\n" +
                          "NHIỆM VỤ:\n" +
                          "1. Chỉ trả lời các câu hỏi liên quan đến học tập, giáo dục và thông tin về các khóa học trong danh sách trên.\n" +
                          "2. Khi nhắc đến tên khóa học, BẮT BUỘC phải kèm theo đường dẫn chi tiết dưới dạng Markdown: [Tên khóa học](/Course/Details/{CourseId}). Ví dụ: [Lập trình Web](/Course/Details/3fa85f64-5717-4562-b3fc-2c963f66afa6).\n" +
                          "3. Nếu người dùng hỏi về các chủ đề không liên quan (chính trị, tôn giáo, giải trí, tư vấn tình cảm, y tế, pháp luật, v.v.) hoặc các nội dung nhạy cảm, hãy từ chối trả lời một cách lịch sự và hướng họ quay lại chủ đề học tập.\n" +
                          "   Ví dụ: 'Xin lỗi, tôi chỉ có thể hỗ trợ bạn các vấn đề liên quan đến khóa học và học tập. Bạn cần tư vấn khóa học nào không?'\n" +
                          "4. Đề xuất các khóa học phù hợp từ danh sách dựa trên nhu cầu của người dùng.\n" +
                          "5. Trả lời ngắn gọn, thân thiện, chuyên nghiệp bằng tiếng Việt."
            }
        };

        // Append history
        if (history != null && history.Any())
        {
            messages.AddRange(history.Select(h => new { role = h.Role, content = h.Content }));
        }

        // Append current question
        messages.Add(new { role = "user", content = question });

        var payload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = messages,
            temperature = 0.5
        };

        return await SendToAi(payload);
    }

    private async Task<string> SendToAi(object payload)
    {
        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);

        try 
        {
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
        catch (Exception ex)
        {
            return $"Xin lỗi, có lỗi xảy ra khi liên hệ với AI server: {ex.Message}";
        }
    }

    public async Task<string> SummarizeTranscriptAsync(string transcript)
    {
        var messages = new List<object>
        {
            new { role = "system", content = "You are an expert educational content summarizer. Analyze the provided video transcript and create a concise, structured summary. Structure the summary with headers (###) and bullet points. Focus on key concepts and learning outcomes. Return ONLY the summary, no intro/outro text." },
            new { role = "user", content = transcript }
        };

        var payload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = messages,
            temperature = 0.3
        };

        return await SendToAi(payload);
    }

    public async Task<string> AskWithContextAsync(string question, string context)
    {
         var messages = new List<object>
        {
            new { role = "system", content = "You are an intelligent AI teaching assistant. " +
                  "Answer the user's question based strictly on the provided lesson transcript. " +
                  "If the question is not related to the lesson content or coding/course topic, politely decline to answer. " +
                  "CRITICAL: When citing information from the transcript, you MUST include the timestamp in the format [mm:ss] (e.g., [02:30], [15:45]). Estimate timestamps based on the text flow if not explicit, or guide the user generally. " +
                  "If you don't know the answer from the context, state that." },
            new { role = "user", content = $"Context (Transcript):\n{context}\n\nQuestion: {question}" }
        };

        var payload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = messages,
            temperature = 0.5
        };

        return await SendToAi(payload);
    }
}
