using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BravoWeb.Models;

public class ContentFragment
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public string HtmlContent { get; set; } = null!;

    public string? CssContent { get; set; }

    public string? JsContent { get; set; }

    public int DisplayOrder { get; set; } = 0;

    // null -> home page, otherwise -> belongs to a SitePage
    public int? PageId { get; set; }

    [ForeignKey("PageId")]
    public SitePage? Page { get; set; }

    // null -> legacy (raw html), otherwise -> template-based
    public int? TemplateId { get; set; }

    [ForeignKey("TemplateId")]
    public CustomTemplate? Template { get; set; }

    // flat json merged into template placeholders, e.g. {"TITLE":"...","BG_IMAGE":"..."}
    public string? DataJson { get; set; }

    /// <summary>
    /// Foreign key to User who created this fragment
    /// </summary>
    public int CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key to User who last updated this fragment
    /// </summary>
    public int? UpdatedById { get; set; }

    [ForeignKey("UpdatedById")]
    public User? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public string Content
    {
        get => HtmlContent;
        set => HtmlContent = value;
    }
}
