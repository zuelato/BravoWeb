using System.Text.Json;

namespace BravoWeb.Services;

/// <summary>
/// Merges a template's HTML with a fragment's DataJson by replacing
/// {{KEY}} placeholders with their corresponding values.
/// </summary>
public class TemplateRenderer
{
    /// <summary>
    /// Replace all {{KEY}} placeholders in <paramref name="templateHtml"/>
    /// with values from <paramref name="dataJson"/>.
    /// If <paramref name="dataJson"/> is null or empty the template HTML is returned as-is.
    /// </summary>
    public string Render(string templateHtml, string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(templateHtml))
            return templateHtml ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dataJson))
            return templateHtml;

        Dictionary<string, string>? data;
        try
        {
            data = JsonSerializer.Deserialize<Dictionary<string, string>>(dataJson);
        }
        catch (JsonException)
        {
            // Malformed JSON — return template unchanged
            return templateHtml;
        }

        if (data is null || data.Count == 0)
            return templateHtml;

        var result = templateHtml;
        foreach (var kvp in data)
        {
            result = result.Replace("{{" + kvp.Key + "}}", kvp.Value, StringComparison.Ordinal);
        }

        return result;
    }
}
