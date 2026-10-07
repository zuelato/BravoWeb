using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BravoWeb.Models;

public class SitePage
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Slug { get; set; } = null!;

    [Required, StringLength(200)]
    public string Title { get; set; } = null!;

    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// Foreign key to User who created this page
    /// </summary>
    public int CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key to User who last updated this page
    /// </summary>
    public int? UpdatedById { get; set; }

    [ForeignKey("UpdatedById")]
    public User? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ContentFragment> Fragments { get; set; } = new List<ContentFragment>();
}
