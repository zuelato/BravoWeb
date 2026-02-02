using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BravoWeb.ViewComponents
{
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