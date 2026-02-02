using System.Threading.Tasks;
using BravoWeb.Data;
using BravoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BravoWeb.Pages.Admin.Fragments
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _db;

        public EditModel(AppDbContext db) => _db = db;

        [BindProperty]
        public ContentFragment Fragment { get; set; } = new ContentFragment();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                Fragment = new ContentFragment();
                return Page();
            }

            Fragment = await _db.ContentFragments.FindAsync(id.Value);
            if (Fragment == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (Fragment.Id == 0)
            {
                _db.ContentFragments.Add(Fragment);
            }
            else
            {
                var existing = await _db.ContentFragments.FindAsync(Fragment.Id);
                if (existing == null) return NotFound();

                existing.Name = Fragment.Name;
                existing.HtmlContent = Fragment.HtmlContent;
                existing.DisplayOrder = Fragment.DisplayOrder;
                _db.Entry(existing).State = EntityState.Modified;
            }

            await _db.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
