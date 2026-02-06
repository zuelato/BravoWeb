using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Models;

namespace BravoWeb.Controllers
{
    public class SitePagesController : Controller
    {
        private readonly AppDbContext _context;

        public SitePagesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SitePages
        public async Task<IActionResult> Index()
        {
            var pages = await _context.SitePages
                .OrderBy(p => p.Title)
                .ToListAsync();
            return View(pages);
        }

        // GET: SitePages/Create
        public IActionResult Create()
        {
            return View(new SitePage());
        }

        // POST: SitePages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Slug,IsPublished")] SitePage page)
        {
            // Normalize slug
            page.Slug = page.Slug?.Trim().ToLowerInvariant().Replace(" ", "-") ?? "";

            if (string.IsNullOrWhiteSpace(page.Slug))
            {
                ModelState.AddModelError("Slug", "Slug is required.");
            }
            else if (await _context.SitePages.AnyAsync(p => p.Slug == page.Slug))
            {
                ModelState.AddModelError("Slug", "A page with this slug already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.SitePages.Add(page);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(page);
        }

        // GET: SitePages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var page = await _context.SitePages.FindAsync(id);
            if (page == null) return NotFound();
            return View(page);
        }

        // POST: SitePages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Slug,IsPublished")] SitePage page)
        {
            if (id != page.Id) return NotFound();

            page.Slug = page.Slug?.Trim().ToLowerInvariant().Replace(" ", "-") ?? "";

            if (string.IsNullOrWhiteSpace(page.Slug))
            {
                ModelState.AddModelError("Slug", "Slug is required.");
            }
            else if (await _context.SitePages.AnyAsync(p => p.Slug == page.Slug && p.Id != page.Id))
            {
                ModelState.AddModelError("Slug", "A page with this slug already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(page);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.SitePages.AnyAsync(p => p.Id == page.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(page);
        }

        // GET: SitePages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var page = await _context.SitePages.FirstOrDefaultAsync(p => p.Id == id);
            if (page == null) return NotFound();
            return View(page);
        }

        // POST: SitePages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _context.SitePages.FindAsync(id);
            if (page != null)
            {
                _context.SitePages.Remove(page); // cascade deletes fragments
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
