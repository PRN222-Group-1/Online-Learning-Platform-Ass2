using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Order;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Order;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IOrderService _orderService;

    public DetailsModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public OrderDetailViewModel? Order { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            Order = await _orderService.GetOrderDetailsAsync(id);
            
            if (Order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToPage("/Order/MyOrders");
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Unable to load order details. Please try again.";
            return RedirectToPage("/Order/MyOrders");
        }

        return Page();
    }
}
