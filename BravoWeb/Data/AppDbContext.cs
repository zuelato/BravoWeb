using System;
using System.Collections.Generic;
using BravoWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BravoWeb.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<Banner> Banners { get; set; }
    public virtual DbSet<ContentFragment> ContentFragments { get; set; }
    public virtual DbSet<SitePage> SitePages { get; set; }
    public virtual DbSet<CustomTemplate> CustomTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SitePage>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<ContentFragment>(entity =>
        {
            entity.HasOne(f => f.Page)
                .WithMany(p => p.Fragments)
                .HasForeignKey(f => f.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            // delete template -> set null on fragments (don't cascade delete)
            entity.HasOne(f => f.Template)
                .WithMany(t => t.Fragments)
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
