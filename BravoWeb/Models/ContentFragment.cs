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

    public int DisplayOrder { get; set; } = 0;

    // Nullable FK: null = home page, otherwise belongs to a SitePage
    public int? PageId { get; set; }

    [ForeignKey("PageId")]
    public SitePage? Page { get; set; }

    [NotMapped]
    public string Content
    {
        get => HtmlContent;
        set => HtmlContent = value;
    }
}
