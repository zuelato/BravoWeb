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
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<SitePage>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();

            // Relationship: SitePage -> CreatedBy (User)
            entity.HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: SitePage -> UpdatedBy (User, nullable)
            entity.HasOne(p => p.UpdatedBy)
                .WithMany()
                .HasForeignKey(p => p.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ContentFragment>(entity =>
        {
            // Relationship: ContentFragment -> Page (cascade delete)
            entity.HasOne(f => f.Page)
                .WithMany(p => p.Fragments)
                .HasForeignKey(f => f.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: ContentFragment -> Template (set null on delete)
            entity.HasOne(f => f.Template)
                .WithMany(t => t.Fragments)
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship: ContentFragment -> CreatedBy (User)
            entity.HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: ContentFragment -> UpdatedBy (User, nullable)
            entity.HasOne(f => f.UpdatedBy)
                .WithMany()
                .HasForeignKey(f => f.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
