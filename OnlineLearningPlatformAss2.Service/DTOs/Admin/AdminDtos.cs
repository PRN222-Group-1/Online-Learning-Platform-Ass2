namespace OnlineLearningPlatformAss2.Service.DTOs.Admin;

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalCourses { get; set; }
    public int PendingCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalNetProfit { get; set; }
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class RecentOrderDto
{
    public Guid OrderId { get; set; }
    public string Username { get; set; } = null!;
    public string ItemTitle { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
