# Content Templates - Implementation Plan

## Overview

Add a template picker to the Create Fragment flow so users can start from a pre-built HTML skeleton instead of a blank editor. Templates are file-based (no database changes needed).

---

## Architecture

### Storage: File-based (recommended)

Store `.html` template files and a manifest in `wwwroot/templates/`:

```
wwwroot/templates/
??? blank.html              (empty)
??? news-article.html       (news layout skeleton)
??? overview-section.html   (hero + description)
??? faq-section.html        (accordion layout)
??? templates.json          (manifest/metadata)
```

**Why file-based?**
- No migration or new model needed
- Easy to version control and edit in VS
- Can migrate to database later if non-developers need to manage templates
- Front-end stays the same either way

### Manifest: `templates.json`

Describes available templates for the picker UI:

```json
[
  { "id": "blank",            "name": "Blank",            "file": "blank.html",            "category": "General",  "description": "Start from scratch" },
  { "id": "news-article",     "name": "News Article",     "file": "news-article.html",     "category": "News",     "description": "Article with title, date, image, and body" },
  { "id": "overview-section", "name": "Overview Section",  "file": "overview-section.html", "category": "Layout",   "description": "Hero banner with description text" },
  { "id": "faq-section",      "name": "FAQ Section",      "file": "faq-section.html",      "category": "Layout",   "description": "Accordion-style Q&A layout" }
]
```

### Template File Example (`news-article.html`)

```html
<div class="news-article">
    <div class="news-header">
        <h2>{{TITLE}}</h2>
        <span class="news-date">{{DATE}}</span>
        <span class="news-tag">{{TAG}}</span>
    </div>
    <div class="news-image">
        <img src="{{IMAGE_URL}}" alt="{{TITLE}}" />
    </div>
    <div class="news-body">
        <p>{{CONTENT}}</p>
    </div>
</div>
```

Placeholders like `{{TITLE}}` are visual guides only - users replace them in Monaco. No server-side rendering needed.

---

## UI Flow

```
User clicks "Create New"
        |
        v
Template picker appears (cards or dropdown)
        |
        v
User picks a template
        |
    +---+---+
    |       |
    v       v
  Blank   Template
    |       |
    v       v
  Monaco   Fetch /templates/{file}.html
  opens      |
  empty      v
           Monaco opens with template HTML pre-filled
             |
             v
        User edits content, clicks Save
             |
             v
        Fragment saved to DB as usual
```

---

## What Needs to Change

| Component | Change | Effort |
|-----------|--------|--------|
| `wwwroot/templates/` | New folder with `.html` files + `templates.json` | Small |
| `ContentFragments/Create` view | Add template picker UI (cards or dropdown) before Monaco | Medium |
| JavaScript in Create view | Fetch `templates.json`, show picker, on selection fetch the `.html` file and set as Monaco editor value | Medium |
| Controller | **No changes** - still receives `Name` + `HtmlContent` | None |
| Database | **No changes** | None |
| Models | **No changes** | None |

---

## Decisions to Make Before Coding

| Decision | Options | Recommendation |
|----------|---------|----------------|
| Picker UI style | Modal with thumbnail cards **or** simple dropdown | Dropdown for now, cards later |
| Template categories | Grouped (News, Layout, General) **or** flat list | Flat list to start |
| Template previews | Name + description only **or** include thumbnail screenshots | Name + description to start |
| Who manages templates | Developer only (edit files) **or** admin UI for templates | Developer only to start |

---

## Future Enhancement: Database-driven Templates

If needed later, migrate to a database approach:

### New Model

```csharp
public class ContentTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public string HtmlTemplate { get; set; }
}
```

### New Migration

Adds `ContentTemplates` table.

### New Admin UI

CRUD pages at `/ContentTemplates` for managing templates.

### Front-end

Replace `fetch('/templates/templates.json')` with `fetch('/api/templates')` - picker UI stays the same.

---

## Summary

- **Phase 1 (now):** File-based templates + dropdown picker. Zero backend changes.
- **Phase 2 (later):** Database-driven templates + admin UI + thumbnail previews.
