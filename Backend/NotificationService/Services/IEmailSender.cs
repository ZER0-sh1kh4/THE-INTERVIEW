namespace NotificationService.Services;

/// <summary>
/// Defines email sending operations for the notification worker.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken);
}
