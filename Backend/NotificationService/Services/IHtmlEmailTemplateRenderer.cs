namespace NotificationService.Services;

/// <summary>
/// Builds simple HTML email bodies from a template key and replacement values.
/// </summary>
public interface IHtmlEmailTemplateRenderer
{
    string Render(string templateKey, IReadOnlyDictionary<string, string> model);
}
