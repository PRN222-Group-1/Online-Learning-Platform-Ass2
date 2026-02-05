namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string from, string message, DateTime sentAt);
    Task AdminJoined(string adminName);
    Task UserRequestedSupport(string connectionId, string userName);
    Task SupportEnded();
}
