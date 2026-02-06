# BravoWeb CMS - Site Creation & Management Documentation

## Overview

BravoWeb includes a simple CMS (Content Management System) that allows trusted admins to:

1. **Create dynamic site pages** with custom URL slugs
2. **Add/edit/delete HTML content fragments** on each page
3. **Reorder content fragments** via drag-and-drop
4. **Preview pages** with the shared site layout (header + footer)

---

## Architecture

### Models

| Model | Table | Purpose |
|-------|-------|---------|
| `SitePage` | `SitePages` | Represents a dynamic page with a URL slug, title, and published flag |
| `ContentFragment` | `ContentFragments` | An HTML block that belongs to a page (or the home page if `PageId` is null) |

**Relationship:** `SitePage` has many `ContentFragment` records. Deleting a page cascades and deletes all its fragments.

### Controllers

| Controller | Route | Purpose |
|------------|-------|---------|
| `HomeController` | `/` | Renders the home page using fragments where `PageId` is null |
| `SitePagesController` | `/SitePages` | CRUD for managing dynamic pages |
| `ContentFragmentsController` | `/ContentFragments` | CRUD for managing content fragments, scoped by `?pageId=` |
| `DynamicPageController` | `/{slug}` | Catch-all route that renders a published page by its slug |

### Route Priority (Program.cs)

1. Static files
2. Razor Pages (`/Admin/Fragments/...`)
3. Default MVC routes (`{controller}/{action}/{id?}`) - handles `/SitePages/Create`, `/ContentFragments/Edit/5`, etc.
4. Dynamic page catch-all (`{slug}`) - only matches if no controller route matched first

---

## How It Works

### Creating a New Page

1. Navigate to `/SitePages`
2. Click **Create New Page**
3. Fill in:
   - **Title** - display name (e.g. "News")
   - **Slug** - URL path (e.g. "news" creates the route `/news`)
   - **IsPublished** - checkbox; only published pages are publicly visible
4. Click **Create**
5. The page now appears in the list at `/SitePages`

### Managing Page Content

1. From `/SitePages`, click **Manage Fragments** next to a page
2. This navigates to `/ContentFragments?pageId=X`
3. Click **Create New** to add a content fragment:
   - **Name** - identifier for the fragment (e.g. "hero-banner")
   - **HTML Content** - edited via Monaco code editor (syntax-highlighted HTML)
4. New fragments are automatically placed at the top (DisplayOrder = 0)
5. **Drag and drop** rows using the handle (three lines icon) to reorder fragments
6. Order is saved automatically via AJAX to `/ContentFragments/Reorder`

### Viewing a Dynamic Page

- Visit `/{slug}` (e.g. `/news`)
- The page renders inside the shared layout (header, navbar, footer)
- All published fragments for that page are rendered in order using `@Html.Raw()`

### Home Page Fragments

- Home page fragments have `PageId = null`
- Managed at `/ContentFragments` (no `pageId` query parameter)
- Also accessible from the link at the bottom of `/SitePages`

---

## Database Schema

### SitePages Table

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| Slug | varchar(100) | Unique index, used as URL path |
| Title | varchar(200) | Page display name |
| IsPublished | boolean | Only published pages are publicly accessible |

### ContentFragments Table

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| Name | varchar(100) | Fragment identifier |
| HtmlContent | text | Raw HTML stored as-is (no sanitization) |
| DisplayOrder | int | Controls rendering order (lower = higher on page) |
| PageId | int (FK, nullable) | References SitePages.Id; null = home page |

---

## Admin URLs Reference

| URL | Purpose |
|-----|---------|
| `/SitePages` | List all dynamic pages |
| `/SitePages/Create` | Create a new page |
| `/SitePages/Edit/{id}` | Edit page title/slug/published |
| `/SitePages/Delete/{id}` | Delete a page and all its fragments |
| `/ContentFragments` | Manage home page fragments |
| `/ContentFragments?pageId={id}` | Manage fragments for a specific page |
| `/ContentFragments/Create?pageId={id}` | Add a fragment to a specific page |
| `/ContentFragments/Edit/{id}` | Edit a fragment's name and HTML |

---

## Security Notes

- No authentication/authorization is currently applied
- The CMS is intended for trusted local admins only
- HTML is stored and rendered as-is (`@Html.Raw()`) with no sanitization
- Add `[Authorize]` attributes to `SitePagesController` and `ContentFragmentsController` before deploying to production

---

## Migration History

| Migration | Changes |
|-----------|---------|
| `AddSitePages` | Added `SitePages` table, added nullable `PageId` FK to `ContentFragments`, unique index on `Slug`, cascade delete |
