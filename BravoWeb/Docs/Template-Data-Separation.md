# Separating Structure from Data — Migration Plan

## Progress Summary

| Phase | Status | Description |
|-------|--------|-------------|
| **Phase 1** | ✅ Complete | Schema changes — `DataJson` + `TemplateId` added to `ContentFragment`, FK to `CustomTemplates`, migration applied |
| **Phase 2** | ✅ Complete | Server-side rendering — `TemplateRenderer` service, template-aware rendering in `Index.cshtml` + `DynamicPage.cshtml` |
| **Phase 3** | ✅ Complete | CMS editor changes — Template mode (data form) vs Raw mode (Monaco) in Create + Edit views |
| **Phase 4** | ✅ Complete | All 4 seed fragments (banner, product_news, testimonials, partners) converted to template-based. 4 CustomTemplate rows seeded. No legacy fragments remain. |

### What was done in each phase:

**Phase 1:**
- Added `TemplateId` (nullable FK → `CustomTemplates`) and `DataJson` (nullable text) to `ContentFragment`
- Configured `OnDelete(DeleteBehavior.SetNull)` so deleting a template doesn't delete fragments
- Added `Fragments` navigation collection to `CustomTemplate`
- Migration: `20260211000000_AddTemplateDataToContentFragment`
- Fixed `DbSyncController` FK ordering (delete fragments before templates, insert templates before fragments)
- Fixed `DbSyncController` to run `pgDb.Database.Migrate()` + `sqliteDb.EnsureDeleted()/EnsureCreated()` before sync

**Phase 2:**
- Created `Services/TemplateRenderer.cs` — simple `{{KEY}}` → value replacement via `string.Replace()`
- Registered `TemplateRenderer` as singleton in `Program.cs`
- Updated `HomeController` + `DynamicPageController` to eagerly load `.Include(f => f.Template)` / `.ThenInclude(f => f.Template)`
- Updated `Views/Home/Index.cshtml` + `Views/DynamicPage/DynamicPage.cshtml`:
  - If `TemplateId` is set → use template HTML merged with DataJson, template CSS/JS
  - If `TemplateId` is null → legacy behavior (fragment's own HTML/CSS/JS)
  - CSS emitted per-fragment (scoped), JS deduplicated per template

**Phase 3:**
- Updated `Views/ContentFragments/Edit.cshtml`:
  - Template mode: generates a data form from `{{PLACEHOLDER}}` tokens in template HTML, pre-fills with current DataJson values, submits `TemplateId` + `DataJson`
  - Raw mode: Monaco editors as before, submits `HtmlContent`/`CssContent`/`JsContent`
- Updated `Views/ContentFragments/Create.cshtml`:
  - When a custom template with `{{PLACEHOLDERS}}` is selected → shows data form (template mode)
  - When blank or no-placeholder template is selected → shows Monaco editors (raw mode)
  - "Create Your Own" flow → raw mode with template metadata fields
- Updated `ContentFragmentsController.Edit()` to eagerly load Template

**Phase 4:**
- Created `CustomTemplate` rows for all 4 block types: Banner, Tin tức sản phẩm, Phản hồi khách hàng, Đối tác
- Created template files: `testimonials.html/css/js`, `partners.html/css` in `wwwroot/templates/`
- Updated `templates.json` manifest with testimonials + partners entries
- Converted all 4 seed fragments in `Program.cs` from raw HTML to `TemplateId` + `DataJson`
- Templates for product-news and banner reuse existing file-based templates
- Templates for testimonials and partners load content from files via `File.ReadAllText()`
- All fragments now have `HtmlContent = "<!-- rendered from template -->"` (placeholder for `[Required]` attribute)
- No legacy (raw HTML) fragments remain in seed data

---

## The Problem

Currently, every `ContentFragment` stores its full HTML as a single blob. If you want the same banner layout on both the Home page and the News page — but with different images, titles, and descriptions — you have to **duplicate the entire HTML** for each page. Change the banner structure later? You have to find and update every copy.

### Current Model

```
ContentFragment
├── Id
├── Name            "banner"
├── HtmlContent     "<div class='banner-container'>...full HTML with data baked in..."
├── CssContent
├── JsContent
├── DisplayOrder
└── PageId          null (home) or 5 (news)
```

Home page banner and News page banner are two completely separate fragments with copy-pasted HTML.

---

## The Goal

Separate **structure** (the HTML/CSS/JS template) from **data** (the values that change per page). Two fragments using the same banner block should reference the **same template** and only differ in their data.

### Target Model (conceptual)

```
Home Page "banner" fragment
├── Template → shared banner template (HTML + CSS + JS with {{placeholders}})
└── Data     → { "TITLE": "Home", "IMAGE": "home.jpg", "CTA_TEXT": "VỀ CHÚNG TÔI" }

News Page "banner" fragment
├── Template → same shared banner template
└── Data     → { "TITLE": "Tin tức", "IMAGE": "news.jpg", "CTA_TEXT": "XEM THÊM" }
```

---

## Approach: Gradual Migration in 4 Phases

### Phase 1 — Schema Changes (DB + Model)

**Add a `DataJson` column to `ContentFragment`:**

```csharp
public class ContentFragment
{
    // ...existing columns...

    /// <summary>
    /// JSON key-value pairs that get merged into the template.
    /// null = legacy fragment (raw HTML, no template).
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// FK to the template that provides the HTML/CSS/JS structure.
    /// null = legacy fragment (uses its own HtmlContent directly).
    /// </summary>
    public int? TemplateId { get; set; }

    [ForeignKey("TemplateId")]
    public CustomTemplate? Template { get; set; }
}
```

**Key decisions:**
- `TemplateId` is **nullable** — existing fragments keep working as-is (no template, raw HTML).
- `DataJson` stores a simple flat JSON object: `{ "TITLE": "...", "IMAGE_URL": "...", "CTA_TEXT": "..." }`.
- `HtmlContent` stays — it becomes the **rendered/resolved** HTML (or stays as raw HTML for legacy fragments).

**Why not remove `HtmlContent`?**
- Backward compatibility — all existing fragments still work.
- Fragments without a template (e.g., fully custom one-off blocks) can still store raw HTML.
- The rendered output can be cached in `HtmlContent` for performance.

**Migration required:** Add `DataJson` (text, nullable) and `TemplateId` (int, nullable, FK to `CustomTemplates`).

---

### Phase 2 — Server-Side Template Rendering

**Create a rendering service** that merges a template's HTML with a fragment's data:

```csharp
public class TemplateRenderer
{
    /// <summary>
    /// Replace all {{KEY}} placeholders in the template HTML with values from the data JSON.
    /// </summary>
    public string Render(string templateHtml, string? dataJson)
    {
        if (string.IsNullOrEmpty(dataJson))
            return templateHtml;

        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(dataJson);
        var result = templateHtml;

        foreach (var kvp in data)
        {
            result = result.Replace("{{" + kvp.Key + "}}", kvp.Value);
        }

        return result;
    }
}
```

**Update the rendering logic** in `Index.cshtml` / `DynamicPage.cshtml`:

```
For each fragment:
    If fragment.TemplateId is not null:
        Get the template's HTML/CSS/JS
        Merge with fragment.DataJson using TemplateRenderer
        Render the merged result
    Else:
        Render fragment.HtmlContent directly (legacy behavior)
```

**Important:** The CSS and JS come from the **template**, not the fragment. The fragment only provides data. But legacy fragments still use their own CSS/JS.

---

### Phase 3 — CMS Editor Changes

**When creating/editing a fragment that uses a template:**

The editor needs two modes:

1. **Template Mode** (when `TemplateId` is set):
   - Show the template's HTML/CSS/JS as **read-only** (or in a preview pane).
   - Show a **data form** — dynamically generated from the `{{PLACEHOLDER}}` tokens found in the template HTML.
   - The user fills in values, not code.

2. **Raw Mode** (legacy, when `TemplateId` is null):
   - Same as today — full Monaco editors for HTML/CSS/JS.

**Placeholder discovery** — scan the template HTML for `{{...}}` patterns:

```javascript
function extractPlaceholders(html) {
    var matches = html.match(/\{\{([A-Z0-9_]+)\}\}/g) || [];
    return [...new Set(matches.map(m => m.replace(/[{}]/g, '')))];
}
// e.g. ["TITLE", "IMAGE_URL", "CTA_TEXT", "SUBTITLE"]
```

Then dynamically build a form with labeled inputs for each placeholder.

**Create flow changes:**

```
User picks a template
    │
    ├─ Built-in or Custom template with placeholders?
    │   → Show data form (input for each {{KEY}})
    │   → On save: store TemplateId + DataJson, leave HtmlContent empty or pre-render it
    │
    └─ Blank / raw template?
        → Show Monaco editors as today (legacy mode)
```

---

### Phase 4 — Migrate Existing Fragments

Once the new system is working, gradually convert existing fragments:

1. **Identify fragments** that share the same HTML structure (e.g., all banners).
2. **Extract** the common HTML into a `CustomTemplate` (or use an existing built-in template).
3. **Replace** each fragment's `HtmlContent` with placeholder-filled template reference.
4. **Set** `TemplateId` and `DataJson` on each fragment.

This can be done one block type at a time. No rush.

---

## Things to Watch Out For

### 1. Backward Compatibility is Non-Negotiable

Every phase must keep existing fragments rendering correctly. The rule:

```
If TemplateId is null → render HtmlContent directly (legacy).
If TemplateId is set  → render template + DataJson merge.
```

Never break the old path.

### 2. Where Does CSS/JS Live?

| Scenario | CSS Source | JS Source |
|----------|-----------|-----------|
| Legacy fragment (no template) | `fragment.CssContent` | `fragment.JsContent` |
| Template-based fragment | `template.CssContent` | `template.JsContent` |

When multiple fragments on the same page use the same template, the CSS/JS should only be emitted **once**. Track which template IDs have already been rendered:

```csharp
var renderedTemplateIds = new HashSet<int>();

foreach (var fragment in fragments)
{
    if (fragment.TemplateId.HasValue && !renderedTemplateIds.Contains(fragment.TemplateId.Value))
    {
        // Emit template CSS/JS
        renderedTemplateIds.Add(fragment.TemplateId.Value);
    }
    // Emit merged HTML
}
```

### 3. DataJson Schema — Keep It Flat

Don't nest objects. Keep it a flat `Dictionary<string, string>`:

```json
{
    "TITLE": "Giải pháp phần mềm quản trị doanh nghiệp",
    "SUBTITLE": "Bravo ERP là sự kết hợp hoàn hảo...",
    "BG_IMAGE": "https://example.com/banner.jpg",
    "CTA_TEXT": "VỀ CHÚNG TÔI",
    "CTA_HREF": "#about"
}
```

**Why flat?**
- Simple `string.Replace()` rendering — no need for a full template engine.
- Easy to generate a form from (one input per key).
- Easy to validate (all values are strings).

### 4. Placeholder Naming Convention

Establish a convention early and stick to it:

| Convention | Example |
|------------|---------|
| UPPER_SNAKE_CASE | `{{SECTION_TITLE}}` |
| Descriptive | `{{CARD_1_IMAGE}}` not `{{IMG1}}` |
| Prefixed for groups | `{{CARD_1_TITLE}}`, `{{CARD_2_TITLE}}` |

The templates already follow this convention (`{{SECTION_LABEL}}`, `{{CARD_1_URL}}`). Keep it consistent.

### 5. The CustomTemplate Table Already Exists

The `CustomTemplate` model already has `HtmlContent`, `CssContent`, `JsContent`. It was built for the "create your own template" feature. The new `TemplateId` FK on `ContentFragment` should point to this table. No need for a new table.

But consider: should **built-in templates** (from `templates.json` files) also be stored in `CustomTemplates` for consistency? Two options:

| Approach | Pros | Cons |
|----------|------|------|
| **A: Keep built-in as files** | No migration of existing templates, fast static serving | Two sources of truth, can't FK to them from fragments |
| **B: Seed built-in into CustomTemplates** | Single source, can FK from fragments, simpler rendering | Need a migration to seed them, lose the file-based simplicity |

**Recommendation:** Go with **B** eventually. For Phase 1, add the FK to `CustomTemplates` and only template-based fragments created via the new flow use it. Migrate built-in templates to the DB later as a cleanup task.

### 6. DbSync Must Handle the New Column

The `DbSyncController` already syncs `CustomTemplates`. Once `ContentFragment` has `TemplateId`, the FK relationship must be preserved during Pull/Push. Since both tables are fully wiped and re-inserted with original IDs, this should work naturally — but **test it**.

### 7. Don't Over-Engineer the Renderer

A simple `string.Replace()` loop is enough. Don't reach for Razor, Scriban, Handlebars, or any template engine. The `{{KEY}}` → value replacement is all you need for this use case.

If you later need conditionals or loops (e.g., "render N cards from a list"), that's a Phase 5 problem. Don't solve it now.

### 8. Preview in the CMS Editor

Once the data form exists, add a live preview panel that shows the rendered HTML. This is a nice-to-have but makes the CMS much more usable. Implementation:

```javascript
function updatePreview() {
    var html = templateHtml; // from the selected template
    var data = collectFormData(); // { "TITLE": "...", ... }
    for (var key in data) {
        html = html.replaceAll('{{' + key + '}}', data[key]);
    }
    document.getElementById('preview-frame').srcdoc = html;
}
```

---

## Summary — Phase Order & Dependencies

```
Phase 1: Schema Changes
    │   Add DataJson + TemplateId to ContentFragment
    │   Migration for PostgreSQL
    │   No behavior changes — everything still works
    │
    ▼
Phase 2: Server-Side Rendering
    │   TemplateRenderer service
    │   Update Home/DynamicPage views
    │   Legacy fragments unchanged
    │
    ▼
Phase 3: CMS Editor Changes
    │   Template mode vs Raw mode in Create/Edit
    │   Data form generated from placeholders
    │   Save as TemplateId + DataJson
    │
    ▼
Phase 4: Migrate Existing Fragments
        Convert copy-pasted fragments to template references
        One block type at a time
```

Each phase is independently deployable. You can stop after any phase and the system works.

---

## Files That Will Be Touched

| Phase | Files |
|-------|-------|
| 1 | `Models/ContentFragment.cs`, `AppDbContext.cs`, new migration, `DbSyncController.cs` |
| 2 | New `Services/TemplateRenderer.cs`, `Views/Home/Index.cshtml`, `Views/DynamicPage/DynamicPage.cshtml`, `HomeController.cs`, `DynamicPageController.cs` |
| 3 | `Views/ContentFragments/Create.cshtml`, `Views/ContentFragments/Edit.cshtml`, `ContentFragmentsController.cs` |
| 4 | Data-only (update existing DB rows via CMS or migration script) |
