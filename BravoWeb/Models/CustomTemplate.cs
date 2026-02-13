using System.ComponentModel.DataAnnotations;

namespace BravoWeb.Models;

public class CustomTemplate
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(10)]
    public string Icon { get; set; } = "🧩";

    [StringLength(200)]
    public string Description { get; set; } = "";

    public string? HtmlContent { get; set; }

    public string? CssContent { get; set; }

    public string? JsContent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fragments that use this template as their structure.
    /// </summary>
    public ICollection<ContentFragment> Fragments { get; set; } = new List<ContentFragment>();
}
