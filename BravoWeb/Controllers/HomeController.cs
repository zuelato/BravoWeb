using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
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
                Url.Content("~/css/testimonials.css"),
                Url.Content("~/css/partner.css")
            };
            ViewData["componentStyles"] = styles;

            var scripts = new List<string>
            {
                Url.Content("~/js/testimonials.js")
            };
            ViewData["componentScripts"] = scripts;

            // load all fragments ordered by DisplayOrder so new fragments appear automatically
            var fragments = _db.ContentFragments
                .AsNoTracking()
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            return View(fragments);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}