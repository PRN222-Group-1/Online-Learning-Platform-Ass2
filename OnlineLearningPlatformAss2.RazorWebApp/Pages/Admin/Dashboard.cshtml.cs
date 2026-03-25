using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.DTOs.Chatbot;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IChatbotService _chatbotService;

    public DashboardModel(IAdminService adminService, IChatbotService chatbotService)
    {
        _adminService = adminService;
        _chatbotService = chatbotService;
    }

    public AdminStatsDto Stats { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    public async Task OnGetAsync()
    {
        Stats = await _adminService.GetStatsAsync(StartDate, EndDate);
    }

    public async Task<IActionResult> OnPostGenerateAiAnalysisAsync([FromBody] DateFilterRequest request)
    {
        Stats = await _adminService.GetStatsAsync(request?.StartDate, request?.EndDate);
        var context = BuildAdminContext(Stats);

        var analysis = await _chatbotService.AnalyzeRevenueAsync(context);
        return new JsonResult(new { analysis });
    }
    public async Task<IActionResult> OnPostAskAdminAsync([FromBody] AdminQuestionRequest request)
    {
        if (string.IsNullOrEmpty(request.Question)) return new JsonResult(new { answer = "" });

        Stats = await _adminService.GetStatsAsync();
        var context = BuildAdminContext(Stats);

        var answer = await _chatbotService.AskAdminAsync(request.Question, context, new List<ChatHistoryItem>());
        return new JsonResult(new { answer });
    }

    private string BuildAdminContext(AdminStatsDto stats)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== TỔNG QUAN HỆ THỐNG ===");
        sb.AppendLine($"Tổng doanh thu: {stats.TotalRevenue:N0} ₫");
        sb.AppendLine($"Lợi nhuận ròng (30%): {stats.TotalNetProfit:N0} ₫");
        sb.AppendLine($"Tổng sinh viên: {stats.TotalUsers}");
        sb.AppendLine($"Tổng giảng viên: {stats.TotalInstructors}");
        sb.AppendLine($"Tổng khóa học: {stats.TotalCourses}");
        sb.AppendLine($"Tổng lượt đăng ký: {stats.TotalEnrollments}");
        sb.AppendLine($"Khóa học chờ duyệt: {stats.PendingCourses}");
        sb.AppendLine();
        sb.AppendLine("=== DỮ LIỆU THEO THÁNG (12 tháng gần nhất) ===");
        foreach (var m in stats.MonthlyChartData)
        {
            sb.AppendLine($"- {m.Month}: {m.EnrollmentCount} sinh viên đăng ký, doanh thu {m.Revenue:N0} ₫");
        }
        return sb.ToString();
    }

    public class AdminQuestionRequest
    {
        public string Question { get; set; } = "";
    }
}

public class DateFilterRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
