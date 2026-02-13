using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Models;

namespace BravoWeb.Controllers
{
    public class ContentFragmentsController : Controller
    {
        private readonly AppDbContext _context;

        public ContentFragmentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ensures DisplayOrder values are sequential (0, 1, 2, ...) for a given page scope.
        /// </summary>
        private async Task NormalizeOrderAsync(int? pageId)
        {
            var ordered = await _context.ContentFragments
                .Where(f => f.PageId == pageId)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Id)
                .ToListAsync();
            for (int i = 0; i < ordered.Count; i++)
            {
                if (ordered[i].DisplayOrder != i)
                {
                    ordered[i].DisplayOrder = i;
                }
            }
            await _context.SaveChangesAsync();
        }

        // GET: ContentFragments?pageId=5
        public async Task<IActionResult> Index(int? pageId)
        {
            await NormalizeOrderAsync(pageId);

            ViewBag.PageId = pageId;
            ViewBag.PageName = "Home Page";

            if (pageId.HasValue)
            {
                var page = await _context.SitePages.FindAsync(pageId.Value);
                if (page != null) ViewBag.PageName = page.Title;
            }

            var fragments = await _context.ContentFragments
                .Where(f => f.PageId == pageId)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            return View(fragments);
        }

        // POST: ContentFragments/Reorder (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds)
        {
            if (orderedIds == null || orderedIds.Count == 0)
                return BadRequest();

            var fragments = await _context.ContentFragments
                .Where(f => orderedIds.Contains(f.Id))
                .ToListAsync();
            var lookup = fragments.ToDictionary(f => f.Id);

            for (int i = 0; i < orderedIds.Count; i++)
            {
                if (lookup.TryGetValue(orderedIds[i], out var fragment))
                {
                    fragment.DisplayOrder = i;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: ContentFragments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var fragment = await _context.ContentFragments.FirstOrDefaultAsync(m => m.Id == id);
            if (fragment == null) return NotFound();
            ViewBag.PageId = fragment.PageId;
            return View(fragment);
        }

        // GET: ContentFragments/Create?pageId=5
        public IActionResult Create(int? pageId)
        {
            var fragment = new ContentFragment { PageId = pageId };
            ViewBag.PageId = pageId;
            return View(fragment);
        }

        // POST: ContentFragments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,HtmlContent,CssContent,JsContent,PageId,TemplateId,DataJson")] ContentFragment contentFragment)
        {
            if (ModelState.IsValid)
            {
                // Push all existing fragments for this page down by 1
                var allFragments = await _context.ContentFragments
                    .Where(f => f.PageId == contentFragment.PageId)
                    .ToListAsync();
                foreach (var f in allFragments)
                {
                    f.DisplayOrder += 1;
                }

                // New fragment goes to position 0 (top)
                contentFragment.DisplayOrder = 0;

                _context.Add(contentFragment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { pageId = contentFragment.PageId });
            }
            ViewBag.PageId = contentFragment.PageId;
            return View(contentFragment);
        }

        // GET: ContentFragments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var fragment = await _context.ContentFragments
                .Include(f => f.Template)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (fragment == null) return NotFound();
            ViewBag.PageId = fragment.PageId;
            return View(fragment);
        }

        // POST: ContentFragments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,HtmlContent,CssContent,JsContent,TemplateId,DataJson")] ContentFragment contentFragment)
        {
            if (id != contentFragment.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.ContentFragments.FirstOrDefaultAsync(f => f.Id == id);
                    if (existing == null) return NotFound();
                    existing.Name = contentFragment.Name;
                    existing.HtmlContent = contentFragment.HtmlContent;
                    existing.CssContent = contentFragment.CssContent;
                    existing.JsContent = contentFragment.JsContent;
                    existing.TemplateId = contentFragment.TemplateId;
                    existing.DataJson = contentFragment.DataJson;
                    // Keep existing DisplayOrder and PageId
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContentFragmentExists(contentFragment.Id)) return NotFound();
                    throw;
                }
                var frag = await _context.ContentFragments.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
                return RedirectToAction(nameof(Index), new { pageId = frag?.PageId });
            }
            return View(contentFragment);
        }

        // GET: ContentFragments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var fragment = await _context.ContentFragments.FirstOrDefaultAsync(m => m.Id == id);
            if (fragment == null) return NotFound();
            ViewBag.PageId = fragment.PageId;
            return View(fragment);
        }

        // POST: ContentFragments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fragment = await _context.ContentFragments.FindAsync(id);
            int? pageId = fragment?.PageId;
            if (fragment != null)
            {
                _context.ContentFragments.Remove(fragment);
                await _context.SaveChangesAsync();
                await NormalizeOrderAsync(pageId);
            }
            return RedirectToAction(nameof(Index), new { pageId });
        }

        private bool ContentFragmentExists(int id)
        {
            return _context.ContentFragments.Any(e => e.Id == id);
        }
    }
}
