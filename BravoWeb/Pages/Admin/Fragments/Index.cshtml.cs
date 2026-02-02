using System.Collections.Generic;
using System.Threading.Tasks;
using BravoWeb.Data;
using BravoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BravoWeb.Pages.Admin.Fragments
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db) => _db = db;

        public IList<ContentFragment> Fragments { get; private set; } = new List<ContentFragment>();

        public async Task OnGetAsync()
        {
            Fragments = await _db.ContentFragments
                .AsNoTracking()
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _db.ContentFragments.FindAsync(id);
            if (item != null)
            {
                _db.ContentFragments.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
