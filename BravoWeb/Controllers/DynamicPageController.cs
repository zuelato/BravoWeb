using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Services;

namespace BravoWeb.Controllers
{
    public class DynamicPageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TemplateRenderer _renderer;

        public DynamicPageController(AppDbContext context, TemplateRenderer renderer)
        {
            _context = context;
            _renderer = renderer;
        }

        // catch-all route → mapped in Program.cs
        public async Task<IActionResult> Show(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return NotFound();

            var page = await _context.SitePages
                .Include(p => p.Fragments.OrderBy(f => f.DisplayOrder))
                    .ThenInclude(f => f.Template)
                .FirstOrDefaultAsync(p => p.Slug == slug.ToLowerInvariant() && p.IsPublished);

            if (page == null) return NotFound();

            ViewBag.Renderer = _renderer;
            return View("DynamicPage", page);
        }
    }
}
