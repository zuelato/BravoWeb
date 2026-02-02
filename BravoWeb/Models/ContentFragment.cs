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

    [NotMapped]
    public string Content
    {
        get => HtmlContent;
        set => HtmlContent = value;
    }
}
