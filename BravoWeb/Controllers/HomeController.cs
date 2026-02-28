using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using BravoWeb.Models;
using BravoWeb.Data;
using BravoWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BravoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;
        private readonly TemplateRenderer _renderer;

        public HomeController(ILogger<HomeController> logger, AppDbContext db, TemplateRenderer renderer)
        {
            _logger = logger;
            _db = db;
            _renderer = renderer;
        }

        public IActionResult Index()
        {
            // home page fragments → pageId null, include template for rendering
            var fragments = _db.ContentFragments
                .AsNoTracking()
                .Include(f => f.Template)
                .Where(f => f.PageId == null)
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            ViewBag.Renderer = _renderer;
            return View(fragments);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}}