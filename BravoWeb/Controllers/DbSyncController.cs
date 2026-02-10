using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BravoWeb.Data;
using BravoWeb.Models;

namespace BravoWeb.Controllers;

public class DbSyncController : Controller
{
    private readonly AppDbContext _activeDb;
    private readonly IConfiguration _config;
    private readonly string _activeProvider;

    public DbSyncController(AppDbContext activeDb, IConfiguration config)
    {
        _activeDb = activeDb;
        _config = config;
        _activeProvider = config.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
    }

    public IActionResult Index()
    {
        ViewBag.ActiveProvider = _activeProvider;
        return View();
    }

    /// <summary>
    /// Pull: PostgreSQL ? SQLite (copy remote data to local file DB)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pull()
    {
        try
        {
            using var pgDb = BuildContext("PostgreSQL");
            using var sqliteDb = BuildContext("SQLite");

            // Ensure SQLite schema exists
            sqliteDb.Database.EnsureCreated();

            // Read everything from PostgreSQL
            var pages = await pgDb.SitePages.AsNoTracking().ToListAsync();
            var fragments = await pgDb.ContentFragments.AsNoTracking().ToListAsync();

            // Wipe SQLite
            sqliteDb.ContentFragments.RemoveRange(sqliteDb.ContentFragments);
            sqliteDb.SitePages.RemoveRange(sqliteDb.SitePages);
            await sqliteDb.SaveChangesAsync();

            // Insert pages first (fragments have FK to pages)
            sqliteDb.SitePages.AddRange(pages);
            await sqliteDb.SaveChangesAsync();

            // Detach pages so fragment FK doesn't cause tracking conflicts
            foreach (var entry in sqliteDb.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;

            sqliteDb.ContentFragments.AddRange(fragments);
            await sqliteDb.SaveChangesAsync();

            TempData["SyncMessage"] = $"Pull complete — {pages.Count} pages, {fragments.Count} fragments copied from PostgreSQL ? SQLite.";
            TempData["SyncSuccess"] = true;
        }
        catch (Exception ex)
        {
            TempData["SyncMessage"] = $"Pull failed: {ex.Message}";
            TempData["SyncSuccess"] = false;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Push: SQLite ? PostgreSQL (copy local changes to remote DB)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Push()
    {
        try
        {
            using var pgDb = BuildContext("PostgreSQL");
            using var sqliteDb = BuildContext("SQLite");

            // Read everything from SQLite
            var pages = await sqliteDb.SitePages.AsNoTracking().ToListAsync();
            var fragments = await sqliteDb.ContentFragments.AsNoTracking().ToListAsync();

            // Wipe PostgreSQL
            pgDb.ContentFragments.RemoveRange(pgDb.ContentFragments);
            pgDb.SitePages.RemoveRange(pgDb.SitePages);
            await pgDb.SaveChangesAsync();

            // Insert pages first
            pgDb.SitePages.AddRange(pages);
            await pgDb.SaveChangesAsync();

            foreach (var entry in pgDb.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;

            pgDb.ContentFragments.AddRange(fragments);
            await pgDb.SaveChangesAsync();

            TempData["SyncMessage"] = $"Push complete — {pages.Count} pages, {fragments.Count} fragments copied from SQLite ? PostgreSQL.";
            TempData["SyncSuccess"] = true;
        }
        catch (Exception ex)
        {
            TempData["SyncMessage"] = $"Push failed: {ex.Message}";
            TempData["SyncSuccess"] = false;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Build a standalone AppDbContext for the given provider ("PostgreSQL" or "SQLite").
    /// </summary>
    private AppDbContext BuildContext(string provider)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        if (provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(_config.GetConnectionString("SqliteConnection"));
        }
        else
        {
            optionsBuilder.UseNpgsql(_config.GetConnectionString("DefaultConnection"));
        }

        var ctx = new AppDbContext(optionsBuilder.Options);
        return ctx;
    }
}
