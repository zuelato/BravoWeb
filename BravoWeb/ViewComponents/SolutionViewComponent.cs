using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BravoWeb.ViewComponents
{
    [ViewComponent(Name = "Solution")]
    public class SolutionViewComponent : ViewComponent
    {
        public SolutionViewComponent() { }
        public Task<IViewComponentResult> InvokeAsync()
        {
            return Task.FromResult<IViewComponentResult>(View());
        }
    }
}