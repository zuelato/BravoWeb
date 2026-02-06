using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;

namespace BravoWeb.Controllers
{
    public class DynamicPageController : Controller
    {
        private readonly AppDbContext _context;

        public DynamicPageController(AppDbContext context)
        {
            _context = context;
        }

        // Mapped via explicit route in Program.cs — not via attribute routing
        public async Task<IActionResult> Show(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return NotFound();

            var page = await _context.SitePages
                .Include(p => p.Fragments.OrderBy(f => f.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Slug == slug.ToLowerInvariant() && p.IsPublished);

            if (page == null) return NotFound();

            return View("DynamicPage", page);
        }
    }
}
