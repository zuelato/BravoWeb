using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BravoWeb.Models;

public class SitePage
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Slug { get; set; } = null!;

    [Required, StringLength(200)]
    public string Title { get; set; } = null!;

    public bool IsPublished { get; set; } = true;

    public ICollection<ContentFragment> Fragments { get; set; } = new List<ContentFragment>();
}
