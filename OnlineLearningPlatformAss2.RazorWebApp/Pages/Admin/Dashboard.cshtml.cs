using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
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

        // Build context string for AI
        var sb = new StringBuilder();
        sb.AppendLine($"=== TỔNG QUAN HỆ THỐNG ===");
        sb.AppendLine($"Tổng doanh thu: {Stats.TotalRevenue:N0} ₫");
        sb.AppendLine($"Lợi nhuận ròng (30%): {Stats.TotalNetProfit:N0} ₫");
        sb.AppendLine($"Tổng sinh viên: {Stats.TotalUsers}");
        sb.AppendLine($"Tổng giảng viên: {Stats.TotalInstructors}");
        sb.AppendLine($"Tổng khóa học: {Stats.TotalCourses}");
        sb.AppendLine($"Tổng lượt đăng ký: {Stats.TotalEnrollments}");
        sb.AppendLine($"Khóa học chờ duyệt: {Stats.PendingCourses}");
        sb.AppendLine();
        sb.AppendLine("=== DỮ LIỆU THEO THÁNG (12 tháng gần nhất) ===");
        foreach (var m in Stats.MonthlyChartData)
        {
            sb.AppendLine($"- {m.Month}: {m.EnrollmentCount} sinh viên đăng ký, doanh thu {m.Revenue:N0} ₫");
        }

        var analysis = await _chatbotService.AnalyzeRevenueAsync(sb.ToString());
        return new JsonResult(new { analysis });
    }
}

public class DateFilterRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
