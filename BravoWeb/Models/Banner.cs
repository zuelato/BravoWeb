using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BravoWeb.Models;

[Keyless]
public partial class Banner
{
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [StringLength(255)]
    public string Subtitle { get; set; } = null!;

    [StringLength(255)]
    public string BackgroundImageUrl { get; set; } = null!;

    [StringLength(255)]
    public string CtaText { get; set; } = null!;

    [StringLength(255)]
    public string CtaHref { get; set; } = null!;
}
