namespace OnlineLearningPlatformAss2.Service.DTOs.VnPay;

public class VnPayRequestModel
{
    public Guid OrderId { get; set; }
    public string FullName { get; set; }
    public string Description { get; set; }
    public double Amount { get; set; }
    public DateTime CreatedDate { get; set; }
}
