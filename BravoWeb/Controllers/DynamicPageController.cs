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

        // Mapped via explicit route in Program.cs — not via attribute routing
        public async Task<IActionResult> Show(string slug)
        {
            var styles = new List<string>
            {
                Url.Content("~/css/banner.css"),
                Url.Content("~/css/product_news.css"),
                Url.Content("~/css/testimonials.css"),
                Url.Content("~/css/partner.css")
            };
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
