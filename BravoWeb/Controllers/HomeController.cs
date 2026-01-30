using System.Diagnostics;
using BravoWeb.Models;
using BravoWeb.Data;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BravoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            // Register only the styles required by the fragments on this page
            var styles = new List<string>
            {
                Url.Content("~/css/banner.css"),
                Url.Content("~/css/product_news.css"),
                Url.Content("~/css/testimonials.css")
            };
            ViewData["componentStyles"] = styles;

            var scripts = new List<string>
            {
                Url.Content("~/js/testimonials.js")
            };
            ViewData["componentScripts"] = scripts;

            // Load banner and product_news from DB
            var fragments = new List<IHtmlContent>();

            var bannerFragment = _db.ContentFragments
                .AsNoTracking()
                .FirstOrDefault(f => f.Name == "banner");
            if (bannerFragment != null)
            {
                fragments.Add(new HtmlString(bannerFragment.HtmlContent));
            }

            // TODO: load testimonials later

            var productNewsFragment = _db.ContentFragments
                .AsNoTracking()
                .FirstOrDefault(f => f.Name == "product_news");
            if (productNewsFragment != null)
            {
                fragments.Add(new HtmlString(productNewsFragment.HtmlContent));
            }

            return View(fragments);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}