using System.Diagnostics;
using BravoWeb.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BravoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Register only the styles required by the fragments on this page
            var styles = new List<string>
            {
                Url.Content("~/css/banner.css"),
                Url.Content("~/css/product_news.css")
            };
            ViewData["componentStyles"] = styles;

            var banner = @"
<div class=""banner-container"" background-image: url(""https://thumbs2.imgbox.com/c5/cf/nBfprcbk_t.jpg"")"""">
        <div class=""banner-container-content"">
            <div class=""banner-container-content-title"">
                <p>Giải pháp phần mềm quản trị doanh nghiệp BRAVO ERP</p>
            </div>
            <div class=""banner-container-content-subtitle"">
                <p>
                    Giải pháp phần mềm quản lý doanh nghiệp BRAVO là sự kết hợp hoàn hảo giữa sản phẩm ""phần mềm"" với những
                    ""kinh nghiệm tư vấn và triển khai phần mềm"" sẽ trở thành ""Bí quyết quản trị doanh nghiệp"" của các doanh nghiệp.
                </p>
            </div>
            <div class=""banner-container-content-about-us"">
                <a class=""cta-link"" href=""#"">
                    <div class=""banner-container-content-button-container"">
                        <p>VỀ CHÚNG TÔI</p>
                    </div>
                </a>
            </div>
        </div>
    </div>";
            var testimonials = @"";
            var product_news = @"
<div class=""product_news-section-container"">
            <div class=""product_news-content"">
                <div class=""upper-section-content"">
                    <div class=""section-name"">
                        <div class=""name"">
                            <p>PRODUCT NEWS</p>
                        </div>
                        <div class=""divider"">
                            <div class=""divider-left""></div>
                            <div class=""divider-right""></div>
                        </div>
                    </div>

                    <div class=""section-title"">
                        <div class=""title"">
                            <p>Tin tức về sản phẩm</p>
                        </div>

                        <a href=""/"" class=""see-all-button"">
                            Xem tất cả
                        </a>
                    </div>
                </div>

                <div class=""product_news-lower-section-content"">
                    <a href=""/"" class=""product_news-news-card"">
                        <div class=""image""></div>
                        <div class=""product_news-news-block"">
                            <div class=""product_news-news-tag"">&news-tag</div>
                            <div class=""product_news-news-title"">&news-title</div>
                            <div class=""product_news-news-desc"">&news-desc</div>
                        </div>
                    </a>

                    <a href=""/"" class=""product_news-news-card"">
                        <div class=""image""></div>
                        <div class=""product_news-news-block"">
                            <div class=""product_news-news-tag"">&news-tag</div>
                            <div class=""product_news-news-title"">&news-title</div>
                            <div class=""product_news-news-desc"">&news-desc</div>
                        </div>
                    </a>

                    <a href=""/"" class=""product_news-news-card"">
                        <div class=""image""></div>
                        <div class=""product_news-news-block"">
                            <div class=""product_news-news-tag"">&news-tag</div>
                            <div class=""product_news-news-title"">&news-title</div>
                            <div class=""product_news-news-desc"">&news-desc</div>
                        </div>
                    </a>
                </div>
		    </div>
        </div>";

            var fragments = new List<IHtmlContent>
            {
                new HtmlString(banner),
                new HtmlString(testimonials),
                new HtmlString(product_news)
            };

            return View(fragments);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}