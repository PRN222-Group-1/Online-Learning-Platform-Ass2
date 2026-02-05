using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Order;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Order;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly IOrderService _orderService;
    private readonly ICourseService _courseService;
    private readonly ILearningPathService _learningPathService;

    public CheckoutModel(
        IOrderService orderService, 
        ICourseService courseService, 
        ILearningPathService learningPathService)
    {
        _orderService = orderService;
        _courseService = courseService;
        _learningPathService = learningPathService;
    }

    [BindProperty]
    public Guid ItemId { get; set; }

    [BindProperty]
    public string ItemType { get; set; } = "Course"; // Course or LearningPath

    [BindProperty]
    public string PaymentMethod { get; set; } = "Credit Card";

    public string ItemTitle { get; set; } = "";
    public decimal ItemPrice { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, string type = "Course")
    {
        ItemId = id;
        ItemType = type;

        if (type == "Course")
        {
            var course = await _courseService.GetCourseDetailsAsync(id);
            if (course == null) return NotFound();
            
            // Check if already enrolled
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out var userId))
            {
                var enrolled = await _courseService.GetEnrolledCoursesAsync(userId);
                if (enrolled.Any(c => c.Id == id))
                {
                    TempData["InfoMessage"] = "You are already enrolled in this course.";
                    return RedirectToPage("/Course/Details", new { id = id });
                }
            }

            ItemTitle = course.Title;
            ItemPrice = course.Price;
        }
        else
        {
            var path = await _learningPathService.GetLearningPathDetailsAsync(id);
            if (path == null) return NotFound();
            ItemTitle = path.Title;
            ItemPrice = path.Price;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        try
        {
            OrderViewModel order;
            if (ItemType == "Course")
            {
                order = await _orderService.CreateCourseOrderAsync(userId, ItemId);
            }
            else
            {
                order = await _orderService.CreateLearningPathOrderAsync(userId, ItemId);
            }

            // Directly process payment for demo / assignment purposes
            var success = await _orderService.ProcessPaymentAsync(order.Id, PaymentMethod);

            if (success)
            {
                TempData["SuccessMessage"] = $"Thank you! Your purchase of \"{ItemTitle}\" was successful.";
                if (ItemType == "Course")
                {
                    return RedirectToPage("/Course/MyCourses");
                }
                else
                {
                    return RedirectToPage("/LearningPath/MyPaths");
                }
            }
            else
            {
                ModelState.AddModelError("", "Payment processing failed. Please try again.");
                return await OnGetAsync(ItemId, ItemType);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return await OnGetAsync(ItemId, ItemType);
        }
    }
}
