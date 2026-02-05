namespace OnlineLearningPlatformAss2.Service.DTOs.Order;

public class OrderViewModel
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string FormattedAmount => $"${TotalAmount:F2}";
    public string FormattedDate => CreatedAt.ToString("MMM dd, yyyy");
    public string StatusBadgeClass => Status switch
    {
        "Completed" => "badge bg-success",
        "Pending" => "badge bg-warning",
        "Failed" => "badge bg-danger",
        _ => "badge bg-secondary"
    };
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderDetailViewModel : OrderViewModel
{
    public List<TransactionViewModel> Transactions { get; set; } = new();
}

public class OrderItemViewModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!; // "Course" or "LearningPath"
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string FormattedPrice => $"${Price:F2}";
}

public class TransactionViewModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FormattedAmount => $"${Amount:F2}";
    public string FormattedDate => CreatedAt.ToString("MMM dd, yyyy HH:mm");
    public string StatusBadgeClass => Status switch
    {
        "Completed" => "badge bg-success",
        "Pending" => "badge bg-warning", 
        "Failed" => "badge bg-danger",
        _ => "badge bg-secondary"
    };
}

public class OrderStatsViewModel
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public string FormattedTotalSpent => $"${TotalSpent:F2}";
}
