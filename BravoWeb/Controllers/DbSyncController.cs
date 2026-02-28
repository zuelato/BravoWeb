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

    // pull: pg -> sqlite
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pull()
    {
        try
        {
            using var pgDb = BuildContext("PostgreSQL");
            pgDb.Database.Migrate();

            var pages = await pgDb.SitePages.AsNoTracking().ToListAsync();
            var fragments = await pgDb.ContentFragments.AsNoTracking().ToListAsync();
            var templates = await pgDb.CustomTemplates.AsNoTracking().ToListAsync();

            using var sqliteDb = BuildContext("SQLite");
            sqliteDb.Database.EnsureDeleted();
            sqliteDb.Database.EnsureCreated();

            // insert order: pages + templates first -> then fragments (fk deps)
            sqliteDb.SitePages.AddRange(pages);
            sqliteDb.CustomTemplates.AddRange(templates);
            await sqliteDb.SaveChangesAsync();

            foreach (var entry in sqliteDb.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;

            sqliteDb.ContentFragments.AddRange(fragments);
            await sqliteDb.SaveChangesAsync();

            TempData["SyncMessage"] = $"Pull complete — {pages.Count} pages, {fragments.Count} fragments, {templates.Count} templates copied from PostgreSQL -> SQLite.";
            TempData["SyncSuccess"] = true;
        }
        catch (Exception ex)
        {
            TempData["SyncMessage"] = $"Pull failed: {ex.Message}";
            TempData["SyncSuccess"] = false;
        }

        return RedirectToAction(nameof(Index));
    }

    // push: sqlite -> pg
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Push()
    {
        try
        {
            using var pgDb = BuildContext("PostgreSQL");
            using var sqliteDb = BuildContext("SQLite");
            pgDb.Database.Migrate();

            var pages = await sqliteDb.SitePages.AsNoTracking().ToListAsync();
            var fragments = await sqliteDb.ContentFragments.AsNoTracking().ToListAsync();
            var templates = await sqliteDb.CustomTemplates.AsNoTracking().ToListAsync();

            // delete order: fragments first -> then templates + pages (fk deps)
            pgDb.ContentFragments.RemoveRange(pgDb.ContentFragments);
            await pgDb.SaveChangesAsync();
            pgDb.CustomTemplates.RemoveRange(pgDb.CustomTemplates);
            pgDb.SitePages.RemoveRange(pgDb.SitePages);
            await pgDb.SaveChangesAsync();

            // insert order: pages + templates first -> then fragments
            pgDb.SitePages.AddRange(pages);
            pgDb.CustomTemplates.AddRange(templates);
            await pgDb.SaveChangesAsync();

            foreach (var entry in pgDb.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;

            pgDb.ContentFragments.AddRange(fragments);
            await pgDb.SaveChangesAsync();

            TempData["SyncMessage"] = $"Push complete — {pages.Count} pages, {fragments.Count} fragments, {templates.Count} templates copied from SQLite -> PostgreSQL.";
            TempData["SyncSuccess"] = true;
        }
        catch (Exception ex)
        {
            TempData["SyncMessage"] = $"Push failed: {ex.Message}";
            TempData["SyncSuccess"] = false;
        }

        return RedirectToAction(nameof(Index));
    }

    // build a standalone dbcontext for the given provider
    private AppDbContext BuildContext(string provider)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        if (provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            optionsBuilder.UseSqlite(_config.GetConnectionString("SqliteConnection"));
        else
            optionsBuilder.UseNpgsql(_config.GetConnectionString("DefaultConnection"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
