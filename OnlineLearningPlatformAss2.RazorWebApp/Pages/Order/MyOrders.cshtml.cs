using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Order;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Order;

[Authorize]
public class MyOrdersModel : PageModel
{
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;

    public MyOrdersModel(IOrderService orderService, IUserService userService)
    {
        _orderService = orderService;
        _userService = userService;
    }

    public IEnumerable<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
    public OrderStatsViewModel Stats { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        try
        {
            Orders = await _orderService.GetUserOrdersAsync(userId);
            Stats = await _orderService.GetOrderStatsAsync(userId);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Unable to load orders. Please try again.";
            Orders = new List<OrderViewModel>();
        }

        return Page();
    }
}
