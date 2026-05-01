using System.Text;

namespace NotificationService.Services;

/// <summary>
/// Renders lightweight HTML email templates used by the system.
/// </summary>
public class HtmlEmailTemplateRenderer : IHtmlEmailTemplateRenderer
{
    /// <summary>
    /// Returns an HTML template for the given email event.
    /// </summary>
    public string Render(string templateKey, IReadOnlyDictionary<string, string> model)
    {
        var title = templateKey switch
        {
            "welcome" => "Welcome to Mock Interview Platform",
            "password-reset-otp" => "Password Reset OTP",
            "payment-success" => "Payment Successful",
            "subscription-upgrade" => "Premium Subscription Activated",
            "assessment-complete" => "Assessment Completed",
            "interview-complete" => "Interview Completed",
            _ => "Mock Interview Platform Notification"
        };

        var body = templateKey switch
        {
            "welcome" => $"<p>Hello {GetValue(model, "FullName")},</p><p>Your account has been created successfully.</p>",
            "password-reset-otp" => $"<p>Hello {GetValue(model, "FullName")},</p><p>Use OTP <strong style=\"font-size:24px;letter-spacing:4px;\">{GetValue(model, "OtpCode")}</strong> to reset your password.</p><p>This OTP will expire in {GetValue(model, "ExpiryMinutes")} minutes.</p>",
            "payment-success" => $"<p>Your payment of {GetValue(model, "Amount")} {GetValue(model, "Currency")} was successful.</p><p>Payment Id: {GetValue(model, "PaymentId")}</p>",
            "subscription-upgrade" => $"<p>Your premium subscription is now active.</p><p>Plan: {GetValue(model, "Plan")}</p><p>Valid until: {GetValue(model, "EndDate")}</p>",
            "assessment-complete" => $"<p>Your assessment for {GetValue(model, "Domain")} is complete.</p><p>Score: {GetValue(model, "Percentage")}%</p><p>Grade: {GetValue(model, "Grade")}</p>",
            "interview-complete" => $"<p>Your interview for {GetValue(model, "Domain")} is complete.</p><p>Score: {GetValue(model, "Percentage")}%</p><p>Grade: {GetValue(model, "Grade")}</p>",
            _ => "<p>You have a new notification from Mock Interview Platform.</p>"
        };

        var builder = new StringBuilder();
        builder.Append("<html><body style=\"font-family:Segoe UI,Arial,sans-serif;background:#f8fafc;padding:24px;\">");
        builder.Append("<div style=\"max-width:640px;margin:0 auto;background:#ffffff;border-radius:12px;padding:24px;border:1px solid #e2e8f0;\">");
        builder.Append($"<h2 style=\"margin-top:0;color:#0f172a;\">{title}</h2>");
        builder.Append(body);
        builder.Append("<p style=\"margin-top:24px;color:#475569;\">Thank you,<br/>Mock Interview Platform</p>");
        builder.Append("</div></body></html>");
        return builder.ToString();
    }

    /// <summary>
    /// Reads a value safely from the template model.
    /// </summary>
    private static string GetValue(IReadOnlyDictionary<string, string> model, string key)
    {
        return model.TryGetValue(key, out var value) ? value : string.Empty;
    }
}
