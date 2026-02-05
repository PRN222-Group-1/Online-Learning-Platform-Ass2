using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly IAdminService _adminService;

    public DashboardModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public AdminStatsDto Stats { get; set; } = new();

    public async Task OnGetAsync()
    {
        Stats = await _adminService.GetStatsAsync();
    }
}
