using System.Text.Json;

namespace BravoWeb.Services;

// merges template html with fragment datajson -> replaces {{KEY}} placeholders
public class TemplateRenderer
{
    // templateHtml + dataJson -> resolved html
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
            // bad json -> return as-is
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
