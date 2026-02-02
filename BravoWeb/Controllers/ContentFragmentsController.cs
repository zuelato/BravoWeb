using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // GET: ContentFragments
        public async Task<IActionResult> Index()
        {
            return View(await _context.ContentFragments.OrderBy(f => f.DisplayOrder).ToListAsync());
        }

        // GET: ContentFragments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var fragment = await _context.ContentFragments.FirstOrDefaultAsync(m => m.Id == id);
            if (fragment == null) return NotFound();
            return View(fragment);
        }

        // GET: ContentFragments/Create
        public IActionResult Create()
        {
            return View(new ContentFragment());
        }

        // POST: ContentFragments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,HtmlContent,DisplayOrder")] ContentFragment contentFragment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contentFragment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contentFragment);
        }

        // GET: ContentFragments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var fragment = await _context.ContentFragments.FindAsync(id);
            if (fragment == null) return NotFound();
            return View(fragment);
        }

        // POST: ContentFragments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,HtmlContent,DisplayOrder")] ContentFragment contentFragment)
        {
            if (id != contentFragment.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contentFragment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContentFragmentExists(contentFragment.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contentFragment);
        }

        // GET: ContentFragments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var fragment = await _context.ContentFragments.FirstOrDefaultAsync(m => m.Id == id);
            if (fragment == null) return NotFound();
            return View(fragment);
        }

        // POST: ContentFragments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fragment = await _context.ContentFragments.FindAsync(id);
            if (fragment != null)
            {
                _context.ContentFragments.Remove(fragment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ContentFragmentExists(int id)
        {
            return _context.ContentFragments.Any(e => e.Id == id);
        }
    }
}
