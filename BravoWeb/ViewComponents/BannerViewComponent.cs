using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BravoWeb.ViewComponents
{
    // Use in views with:
    // @await Component.InvokeAsync("Banner")
    // or with parameters:
    // @await Component.InvokeAsync("Banner", new { title = "My Title", subtitle = "My subtitle", ctaText = "VỀ CHÚNG TÔI", ctaHref = "#" })
    [ViewComponent(Name = "Banner")]

    public class BannerViewComponent : ViewComponent
    {
        public BannerViewComponent()
        {
        }

        public Task<IViewComponentResult> InvokeAsync(
            string? title = null,
            string? subtitle = null,
            string? ctaText = null,
            string? ctaHref = null)
        {
            var model = new BannerViewModel
            {
                Title = title,
                Subtitle = subtitle,
                CtaText = ctaText,
                CtaHref = ctaHref
            };

            return Task.FromResult<IViewComponentResult>(View(model));
        }

        public class BannerViewModel
        {
            public string? Title { get; set; }
            public string? Subtitle { get; set; }
            public string? CtaText { get; set; }
            public string? CtaHref { get; set; }
        }
    }
}