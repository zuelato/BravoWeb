using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BravoWeb.ViewComponents
{
    [ViewComponent(Name = "Partners")]

    public class PartnerViewComponent : ViewComponent
    {
        public PartnerViewComponent() { }

        public Task<IViewComponentResult> InvokeAsync(IEnumerable<string>? logos = null)
        {
            var list = (logos ?? Enumerable.Range(1, 24).Select(_ => "&client-logo")).ToList();
            return Task.FromResult<IViewComponentResult>(View(list));
        }
    }
}