using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BravoWeb.Models;

public class ContentFragment
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = null!; // e.g. "banner", "product_news"

    [Required]
    public string HtmlContent { get; set; } = null!;

    public string? CssContent { get; set; }

    public string? JsContent { get; set; }

    public int DisplayOrder { get; set; } = 0;

    // Nullable FK: null = home page, otherwise belongs to a SitePage
    public int? PageId { get; set; }

    [ForeignKey("PageId")]
    public SitePage? Page { get; set; }

    /// <summary>
    /// FK to the template that provides the HTML/CSS/JS structure.
    /// null = legacy fragment (uses its own HtmlContent directly).
    /// </summary>
    public int? TemplateId { get; set; }

    [ForeignKey("TemplateId")]
    public CustomTemplate? Template { get; set; }

    /// <summary>
    /// JSON key-value pairs that get merged into the template's placeholders.
    /// null = legacy fragment (raw HTML, no template).
    /// Example: { "TITLE": "Home", "BG_IMAGE": "home.jpg" }
    /// </summary>
    public string? DataJson { get; set; }

    [NotMapped]
    public string Content
    {
        get => HtmlContent;
        set => HtmlContent = value;
    }
}
