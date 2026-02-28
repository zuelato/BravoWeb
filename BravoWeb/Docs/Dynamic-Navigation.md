# Dynamic Navigation with Dropdown Menus — Design Document

## Goal

Allow CMS users to fully customise the site's navigation bar — including **dropdown menus** that appear on hover. Each top-level nav item can optionally have child items that form a dropdown panel. The resulting URLs follow a hierarchical pattern: `/news/bravo-news`, `/products/erp`, etc.

---

## Current State

The navbar in `_Layout.cshtml` is **hardcoded HTML**:

```html
<li class="nav-item">
    <a class="nav-link" href="...">GIỚI THIỆU</a>
</li>
<li class="nav-item">
    <a class="nav-link" href="...">TIN TỨC</a>
</li>
...
```

- No CMS control — changing a tab name or adding a new one requires a code deploy.
- No dropdowns — hovering does nothing.
- URLs are scattered between hardcoded MVC routes and dynamic `SitePage` slugs.

---

## Proposed Solution

### New Model: `NavMenuItem`

A single self-referencing table stores the full menu tree.

```
NavMenuItem
├── Id              int, PK
├── Label           string          "TIN TỨC"
├── Slug            string?         "news"           (used to build the URL segment)
├── Url             string?         "/news"          (explicit URL override — if set, Slug is ignored)
├── ParentId        int? FK → self  null = top-level, otherwise = child of another item
├── PageId          int? FK → SitePage   links to a dynamic page (optional)
├── DisplayOrder    int             controls left-to-right (top-level) or top-to-bottom (dropdown) order
├── IsVisible       bool            hide without deleting
├── OpenInNewTab    bool            target="_blank"
├── CssClass        string?         optional extra class for styling (e.g., "nav-highlight")
│
├── Parent          NavMenuItem?         navigation property
├── Children        List<NavMenuItem>    navigation property
└── Page            SitePage?            navigation property
```

### Key Design Decisions

#### 1. URL Resolution — How the final `href` is determined

Each menu item's link is resolved in priority order:

| Priority | Source | Example |
|----------|--------|---------|
| 1 | `Url` (explicit) | `https://facebook.com/bravo` or `/contact` |
| 2 | `PageId` → `SitePage.Slug` | PageId=5 → page slug `bravo-news` → `/bravo-news` |
| 3 | `Slug` (builds path from parent chain) | Parent slug `news` + child slug `bravo-news` → `/news/bravo-news` |
| 4 | `#` (no link, just a dropdown trigger) | Top-level "TIN TỨC" with no page, just children |

The resolver logic (pseudo-code):

```csharp
string ResolveUrl(NavMenuItem item)
{
    if (!string.IsNullOrEmpty(item.Url))
        return item.Url;                          // explicit URL wins

    if (item.PageId.HasValue && item.Page != null)
        return "/" + item.Page.Slug;              // linked SitePage

    if (!string.IsNullOrEmpty(item.Slug))
    {
        // Build hierarchical path: walk up the parent chain
        var segments = new List<string>();
        var current = item;
        while (current != null)
        {
            if (!string.IsNullOrEmpty(current.Slug))
                segments.Insert(0, current.Slug);
            current = current.Parent;
        }
        return "/" + string.Join("/", segments);  // e.g., /news/bravo-news
    }

    return "#";                                   // dropdown trigger, no navigation
}
```

#### 2. Hierarchical URLs — `/news/bravo-news`

When a child item has `Slug = "bravo-news"` and its parent has `Slug = "news"`, the URL becomes `/news/bravo-news`.

**Routing:** The existing dynamic-page catch-all route (`{slug}`) only handles single-segment slugs. For hierarchical URLs, two options:

| Option | Route Pattern | Pros | Cons |
|--------|--------------|------|------|
| **A: Flat SitePage slugs** | `{slug}` = `"news-bravo-news"` | No routing changes needed | URLs are flat, not hierarchical |
| **B: Multi-segment catch-all** | `{**slug}` = `"news/bravo-news"` | Clean hierarchical URLs | Need to update SitePage model + dynamic page controller |

**Recommendation: Option B** — update the catch-all route to `{**slug}` and allow `SitePage.Slug` to contain `/` (e.g., `"news/bravo-news"`). The nav menu system auto-generates these slugs when creating child items linked to pages.

Route change in `Program.cs`:

```csharp
// Before:
app.MapControllerRoute("dynamic-page", "{slug}", ...);

// After:
app.MapControllerRoute("dynamic-page", "{**slug}", ...);
```

#### 3. Only Two Levels Deep

For simplicity and UX, enforce **max 2 levels**: top-level items + one level of dropdown children. No nested sub-menus. This is enforced in the CMS editor (validation), not in the schema (the self-referencing FK supports unlimited depth if needed later).

#### 4. Dropdown Behaviour — CSS Hover

The dropdown appears on hover (desktop) and tap (mobile). Pure CSS + minimal JS for mobile toggle. No complex JavaScript dropdown library needed.

```css
.nav-item-has-children:hover > .nav-dropdown {
    display: flex;
}
```

---

## Schema & Migration

### New Table: `NavMenuItems`

```sql
CREATE TABLE "NavMenuItems" (
    "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
    "Label"         TEXT    NOT NULL,
    "Slug"          TEXT    NULL,
    "Url"           TEXT    NULL,
    "ParentId"      INTEGER NULL REFERENCES "NavMenuItems"("Id") ON DELETE CASCADE,
    "PageId"        INTEGER NULL REFERENCES "SitePages"("Id") ON DELETE SET NULL,
    "DisplayOrder"  INTEGER NOT NULL DEFAULT 0,
    "IsVisible"     BOOLEAN NOT NULL DEFAULT 1,
    "OpenInNewTab"  BOOLEAN NOT NULL DEFAULT 0,
    "CssClass"      TEXT    NULL
);
```

- `ON DELETE CASCADE` for `ParentId`: deleting a top-level item deletes its children.
- `ON DELETE SET NULL` for `PageId`: deleting a page doesn't delete the menu item (it just becomes a `#` link).

### DbContext Addition

```csharp
public DbSet<NavMenuItem> NavMenuItems { get; set; }

// In OnModelCreating:
modelBuilder.Entity<NavMenuItem>(entity =>
{
    entity.HasOne(n => n.Parent)
        .WithMany(n => n.Children)
        .HasForeignKey(n => n.ParentId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(n => n.Page)
        .WithMany()
        .HasForeignKey(n => n.PageId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

### DbSync Update

Add `NavMenuItems` to Pull/Push in `DbSyncController`:

- **Delete order:** NavMenuItems (children first via CASCADE, then parents) → ContentFragments → ...
- **Insert order:** SitePages → CustomTemplates → NavMenuItems (parents first, then children) → ContentFragments

Since the FK is self-referencing, insertion must be ordered: parents (`ParentId == null`) first, then children.

---

## How It Renders in `_Layout.cshtml`

Replace the hardcoded `<ul>` with a ViewComponent or directly query the menu in the layout.

### Option: ViewComponent (recommended)

```csharp
public class NavMenuViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public NavMenuViewComponent(AppDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _db.NavMenuItems
            .Include(n => n.Children.Where(c => c.IsVisible).OrderBy(c => c.DisplayOrder))
            .Include(n => n.Page)
            .Where(n => n.ParentId == null && n.IsVisible)
            .OrderBy(n => n.DisplayOrder)
            .ToListAsync();

        return View(items);
    }
}
```

Layout usage:

```html
<div class="navbar-collapse collapse">
    @await Component.InvokeAsync("NavMenu")
</div>
```

ViewComponent view (`Views/Shared/Components/NavMenu/Default.cshtml`):

```html
<ul class="navbar-nav">
    @foreach (var item in Model)
    {
        var hasChildren = item.Children.Any();
        var href = ResolveUrl(item);

        <li class="nav-item @(hasChildren ? "nav-item-has-children" : "")">
            <a class="nav-link @item.CssClass" href="@href"
               @(item.OpenInNewTab ? "target=\"_blank\"" : "")>
                @item.Label
            </a>

            @if (hasChildren)
            {
                <div class="nav-dropdown">
                    @foreach (var child in item.Children)
                    {
                        var childHref = ResolveChildUrl(item, child);
                        <a class="nav-dropdown-item @child.CssClass" href="@childHref"
                           @(child.OpenInNewTab ? "target=\"_blank\"" : "")>
                            @child.Label
                        </a>
                    }
                </div>
            }
        </li>
    }
</ul>
```

---

## CMS Admin Pages

### Menu Editor (`/admin/nav-menu` or MVC controller)

A simple CRUD interface:

| View | Purpose |
|------|---------|
| **Index** | List all top-level items with their children indented below. Drag-to-reorder (or arrows). |
| **Create** | Form: Label, Slug, Url, Parent (dropdown of existing top-level items or "None"), PageId (dropdown of SitePages), DisplayOrder, IsVisible, OpenInNewTab, CssClass |
| **Edit** | Same form, pre-filled |
| **Delete** | Confirmation — warns that children will be deleted too |

### Auto-generating SitePages from Nav Items

When creating a child nav item (e.g., "Tin BRAVO" under "TIN TỨC"), the CMS could optionally **auto-create** a `SitePage` with slug `"news/bravo-news"` and link it via `PageId`. This avoids having to manually create the page separately.

Flow:

```
User creates nav child item:
    Label: "Tin BRAVO"
    Parent: "TIN TỨC" (slug: "news")
    Slug: "bravo-news"
    
    [x] Auto-create page? (checkbox)
    
    → Creates SitePage { Slug = "news/bravo-news", Title = "Tin BRAVO" }
    → Sets PageId on the nav item
    → User can then add fragments to that page via the existing CMS
```

---

## Dropdown CSS

```css
/* Container for top-level items with children */
.nav-item-has-children {
    position: relative;
}

/* Dropdown panel — hidden by default */
.nav-dropdown {
    display: none;
    position: absolute;
    top: 100%;
    left: 0;
    min-width: 220px;
    background: #fff;
    box-shadow: 0 8px 24px rgba(0,0,0,0.12);
    border-radius: 0 0 8px 8px;
    flex-direction: column;
    z-index: 1000;
    padding: 8px 0;
}

/* Show on hover */
.nav-item-has-children:hover > .nav-dropdown {
    display: flex;
}

/* Individual dropdown link */
.nav-dropdown-item {
    padding: 10px 20px;
    color: #333;
    text-decoration: none;
    font-size: 14px;
    transition: background-color 0.15s;
}

.nav-dropdown-item:hover {
    background-color: #f5f5f5;
    color: #00a88e;
}
```

---

## Seed Data — Initial Menu

```csharp
// Top-level items
var menuItems = new[]
{
    new NavMenuItem { Label = "GIỚI THIỆU",  Slug = "about",       DisplayOrder = 0 },
    new NavMenuItem { Label = "SẢN PHẨM",    Slug = "products",    DisplayOrder = 1 },
    new NavMenuItem { Label = "DỊCH VỤ",     Slug = "services",    DisplayOrder = 2 },
    new NavMenuItem { Label = "TIN TỨC",     Slug = "news",        DisplayOrder = 3 },
    new NavMenuItem { Label = "KHÁCH HÀNG",  Slug = "customers",   DisplayOrder = 4 },
    new NavMenuItem { Label = "TUYỂN DỤNG",  Slug = "careers",     DisplayOrder = 5 },
    new NavMenuItem { Label = "LIÊN KẾT",    Slug = "links",       DisplayOrder = 6 },
    new NavMenuItem { Label = "LIÊN HỆ",     Slug = "contact",     DisplayOrder = 7 },
};

// Example children for "TIN TỨC"
var newsItem = menuItems[3]; // TIN TỨC
var newsChildren = new[]
{
    new NavMenuItem { Label = "Tin BRAVO",      Slug = "bravo-news",   Parent = newsItem, DisplayOrder = 0 },
    new NavMenuItem { Label = "Tin tổng hợp",   Slug = "general-news", Parent = newsItem, DisplayOrder = 1 },
    new NavMenuItem { Label = "Tin sản phẩm",   Slug = "product-news", Parent = newsItem, DisplayOrder = 2 },
};
// URLs: /news/bravo-news, /news/general-news, /news/product-news
```

---

## Implementation Phases

### Phase 1 — Schema & Model
- Create `NavMenuItem` model
- Add `DbSet<NavMenuItem>` + relationships in `AppDbContext`
- Migration
- Update `DbSyncController` to sync `NavMenuItems`
- Seed initial menu data

### Phase 2 — Rendering
- Create `NavMenuViewComponent` with URL resolution logic
- Create ViewComponent view with dropdown HTML
- Replace hardcoded navbar in `_Layout.cshtml` with `@await Component.InvokeAsync("NavMenu")`
- Add dropdown CSS
- Update routing: change `{slug}` to `{**slug}` for hierarchical URLs

### Phase 3 — CMS Editor
- Create `NavMenuController` (or Razor Pages under `/admin/nav-menu`)
- CRUD views: Index (tree list), Create, Edit, Delete
- Parent picker dropdown (only top-level items)
- Optional SitePage auto-creation when creating child items
- Reorder support (display order arrows or drag-and-drop)

### Phase 4 — Polish
- Mobile responsive dropdown (tap-to-toggle instead of hover)
- Active state highlighting (highlight current page's menu item)
- Transition animations for dropdown open/close
- Validate max depth = 2 in CMS

---

## Files That Will Be Touched

| Phase | Files |
|-------|-------|
| 1 | New `Models/NavMenuItem.cs`, `AppDbContext.cs`, new migration, `DbSyncController.cs`, `Program.cs` (seed) |
| 2 | New `ViewComponents/NavMenuViewComponent.cs`, new `Views/Shared/Components/NavMenu/Default.cshtml`, `_Layout.cshtml`, new CSS, `Program.cs` (route change) |
| 3 | New controller or Razor Pages for nav menu CRUD, new views |
| 4 | CSS updates, JS for mobile, view tweaks |

---

## Things to Watch Out For

### 1. Route Conflict with MVC Controllers
The catch-all `{**slug}` route must remain **lowest priority**. A request to `/news/bravo-news` should:
1. First try MVC controllers (Home, ContentFragments, etc.)
2. If no controller matches → try `DynamicPageController.Show(slug: "news/bravo-news")`

Since the default MVC route `{controller=Home}/{action=Index}/{id?}` is mapped first, this works naturally.

### 2. SitePage Slug Uniqueness
Currently `SitePage.Slug` has a unique index. Hierarchical slugs like `"news/bravo-news"` are still unique strings — no conflict. But slashes in slugs need to be handled in the CMS (auto-generate from parent chain, not user-typed).

### 3. Self-Referencing FK in DbSync
When syncing `NavMenuItems`, insert order matters:
1. Insert items where `ParentId == null` first
2. Then insert items where `ParentId != null`

Or: temporarily disable FK checks during bulk insert (database-specific).

### 4. Caching
The nav menu is rendered on every page. Consider caching the menu query result with `IMemoryCache` (invalidate on CMS save). This is a Phase 4 optimisation — not needed initially since the query is tiny.

### 5. Don't Over-Engineer the Dropdown
Start with pure CSS `:hover`. Don't add JavaScript dropdown logic, animation libraries, or mega-menu panels. Keep it simple — a vertical list of links that appears on hover.
