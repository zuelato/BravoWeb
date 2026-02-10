using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Models;

namespace BravoWeb.Controllers;

[Route("api/custom-templates")]
[ApiController]
public class CustomTemplatesApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomTemplatesApiController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET /api/custom-templates
    /// Returns all custom templates as picker-card metadata.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _context.CustomTemplates
            .OrderBy(t => t.CreatedAt)
            .Select(t => new
            {
                id = "custom-" + t.Id,
                t.Name,
                t.Icon,
                t.Description,
                isCustom = true
            })
            .ToListAsync();

        return Ok(templates);
    }

    /// <summary>
    /// GET /api/custom-templates/5
    /// Returns the full content of a single custom template.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var t = await _context.CustomTemplates.FindAsync(id);
        if (t == null) return NotFound();

        return Ok(new
        {
            t.Id,
            t.Name,
            html = t.HtmlContent ?? "",
            css = t.CssContent ?? "",
            js = t.JsContent ?? ""
        });
    }

    /// <summary>
    /// POST /api/custom-templates
    /// Saves a new custom template from the editor content.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomTemplateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { error = "Template name is required." });

        var template = new CustomTemplate
        {
            Name = req.Name,
            Icon = string.IsNullOrWhiteSpace(req.Icon) ? "🧩" : req.Icon,
            Description = req.Description ?? "",
            HtmlContent = req.Html ?? "",
            CssContent = req.Css ?? "",
            JsContent = req.Js ?? ""
        };

        _context.CustomTemplates.Add(template);
        await _context.SaveChangesAsync();

        return Ok(new { id = template.Id, message = "Template saved." });
    }

    /// <summary>
    /// DELETE /api/custom-templates/5
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _context.CustomTemplates.FindAsync(id);
        if (t == null) return NotFound();

        _context.CustomTemplates.Remove(t);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Template deleted." });
    }
}

public class CreateCustomTemplateRequest
{
    public string Name { get; set; } = "";
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public string? Html { get; set; }
    public string? Css { get; set; }
    public string? Js { get; set; }
}
