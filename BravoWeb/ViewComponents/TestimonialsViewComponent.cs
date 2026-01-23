using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BravoWeb.ViewComponents
{
    [ViewComponent(Name = "Testimonials")]

    public class TestimonialsViewComponent : ViewComponent
    {
        public TestimonialsViewComponent() { }
        public Task<IViewComponentResult> InvokeAsync()
        {
            return Task.FromResult<IViewComponentResult>(View());
        }
    }
}