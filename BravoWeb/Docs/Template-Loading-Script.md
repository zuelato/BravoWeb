# Template Loading Script - Technical Reference

## Overview

The Create Fragment page (`Views/ContentFragments/Create.cshtml`) uses a client-side script to load content templates into the Monaco editors. Templates are entirely **file-based** — no API endpoints or database tables involved. The script reads a JSON manifest, builds picker cards, and fetches template files on demand.

---

## File Structure

```
wwwroot/templates/
├── templates.json          ← manifest (describes all available templates)
├── blank.html              ← empty file
├── product-news.html       ← HTML skeleton
├── product-news.css        ← CSS skeleton
└── (future templates...)
```

---

## Manifest Format — `templates.json`

The manifest is a JSON array where each entry describes one template:

```json
[
  {
    "id": "blank",
    "name": "Trống",
    "icon": "📄",
    "description": "Bắt đầu từ đầu với editor trống.",
    "files": {}
  },
  {
    "id": "product-news",
    "name": "Tin tức sản phẩm",
    "icon": "📰",
    "description": "Khối tin tức dạng card 3 cột với tiêu đề, tag, và mô tả.",
    "files": {
      "html": "product-news.html",
      "css": "product-news.css"
    }
  }
]
```

### Field Reference

| Field         | Type     | Required | Description |
|---------------|----------|----------|-------------|
| `id`          | `string` | Yes      | Unique identifier used by `selectTemplate()` |
| `name`        | `string` | Yes      | Display name shown on the picker card |
| `icon`        | `string` | Yes      | Emoji or character shown as the card icon |
| `description` | `string` | Yes      | Short description shown below the card title |
| `files`       | `object` | Yes      | Map of editor tab → filename. Keys: `html`, `css`, `js`. Omitted keys default to empty content. |

### `files` Object

| Key    | Maps to Editor | Example Value        |
|--------|----------------|----------------------|
| `html` | HTML tab       | `"product-news.html"` |
| `css`  | CSS tab        | `"product-news.css"`  |
| `js`   | JavaScript tab | `"product-news.js"`   |

If a key is missing or the `files` object is empty (like the `blank` template), the corresponding Monaco editor opens with empty content.

---

## Script Lifecycle

The script runs in three phases:

### Phase 1 — Load Manifest & Build Picker Cards

Runs immediately on page load via an IIFE (Immediately Invoked Function Expression).

```
Page loads
    │
    ▼
fetch('/templates/templates.json')
    │
    ▼
Parse JSON → store in `templateManifest` array
    │
    ▼
For each template entry:
    Create a .template-card <div>
    Set onclick → selectTemplate(template.id)
    Inject into #templateCards container
```

**Code:**

```js
(function () {
    fetch('/templates/templates.json')
        .then(function (r) { return r.json(); })
        .then(function (templates) {
            templateManifest = templates;
            var container = document.getElementById('templateCards');

            templates.forEach(function (t) {
                var card = document.createElement('div');
                card.className = 'template-card';
                card.onclick = function () { selectTemplate(t.id); };
                card.innerHTML =
                    '<div class="template-icon">' + t.icon + '</div>' +
                    '<h5>' + t.name + '</h5>' +
                    '<p>' + t.description + '</p>';
                container.appendChild(card);
            });
        });
})();
```

### Phase 2 — Template Selection & File Fetching

Triggered when the user clicks a template card.

```
User clicks a card
    │
    ▼
selectTemplate(templateId)
    │
    ▼
Find matching entry in templateManifest
    │
    ▼
Read its `files` object → { html, css, js }
    │
    ▼
Promise.all([
    fetchTemplateFile(files.html),    ← fetch or resolve('')
    fetchTemplateFile(files.css),
    fetchTemplateFile(files.js)
])
    │
    ▼
Pass results to initEditors(html, css, js)
```

**`fetchTemplateFile` helper:**

```js
function fetchTemplateFile(filename) {
    if (!filename) return Promise.resolve('');
    return fetch('/templates/' + filename)
        .then(function (r) { return r.ok ? r.text() : ''; })
        .catch(function () { return ''; });
}
```

- Returns a `Promise<string>`.
- If `filename` is `undefined`/`null`/empty → resolves immediately with `''`.
- If the fetch fails or returns non-200 → resolves with `''` (never rejects).

### Phase 3 — Editor Initialisation

After file content is fetched, `initEditors` creates the three Monaco editor instances.

```
initEditors(initialHtml, initialCss, initialJs)
    │
    ▼
Hide #templatePicker, show #editorSection
    │
    ▼
Disable Save button (prevents submit before editors are ready)
    │
    ▼
require(['vs/editor/editor.main'], function () {
    │
    ├─ Create htmlEditor  (language: 'html')
    ├─ Create cssEditor   (language: 'css')
    ├─ Create jsEditor    (language: 'javascript')
    │
    ▼
    Enable Save button
    │
    ▼
    Attach 'shown.bs.tab' listeners → call .layout() on all editors
});
```

The `shown.bs.tab` listener is necessary because Monaco editors initialised inside a hidden Bootstrap tab pane have zero dimensions. Calling `.layout()` when the tab becomes visible forces Monaco to recalculate its size.

---

## Form Submission — `syncEditors()`

Called via `onsubmit="return syncEditors()"` on the `<form>`.

```js
function syncEditors() {
    if (htmlEditor) document.getElementById('htmlContent').value = htmlEditor.getValue();
    if (cssEditor)  document.getElementById('cssContent').value  = cssEditor.getValue();
    if (jsEditor)   document.getElementById('jsContent').value   = jsEditor.getValue();
    return true;
}
```

Monaco editors maintain their own internal model — the hidden `<textarea>` elements that ASP.NET model-binds from are **not** kept in sync automatically. `syncEditors()` copies each editor's current value into its corresponding `<textarea>` just before the form submits.

---

## Adding a New Template

1. **Create the template files** in `wwwroot/templates/`:

   ```
   wwwroot/templates/
   ├── my-template.html
   ├── my-template.css      (optional)
   └── my-template.js       (optional)
   ```

2. **Add an entry** to `templates.json`:

   ```json
   {
     "id": "my-template",
     "name": "My Template",
     "icon": "🧩",
     "description": "Short description of what this template provides.",
     "files": {
       "html": "my-template.html",
       "css": "my-template.css",
       "js": "my-template.js"
     }
   }
   ```

3. **Done.** No code changes, no rebuild, no migration. The picker card appears automatically on next page load.

### Placeholder Convention

Use `{{PLACEHOLDER_NAME}}` in template files as visual guides for the user:

```html
<h2>{{TITLE}}</h2>
<p>{{DESCRIPTION}}</p>
<div style="background-image: url('{{IMAGE_URL}}');"></div>
```

These are **not** processed server-side. The user replaces them manually in the Monaco editor.

---

## Key Variables

| Variable           | Scope  | Type                  | Description |
|--------------------|--------|-----------------------|-------------|
| `templateManifest` | Global | `Array<Object>`       | Parsed contents of `templates.json` |
| `htmlEditor`       | Global | `monaco.editor.IStandaloneCodeEditor` | Monaco instance for HTML tab |
| `cssEditor`        | Global | `monaco.editor.IStandaloneCodeEditor` | Monaco instance for CSS tab |
| `jsEditor`         | Global | `monaco.editor.IStandaloneCodeEditor` | Monaco instance for JavaScript tab |

## Key Functions

| Function                       | Description |
|--------------------------------|-------------|
| `(IIFE at page load)`         | Fetches `templates.json`, builds picker cards |
| `fetchTemplateFile(filename)` | Fetches a single template file, returns `Promise<string>` |
| `selectTemplate(templateId)`  | Looks up template in manifest, fetches its files, calls `initEditors` |
| `initEditors(html, css, js)`  | Creates Monaco editors with the given content, wires up tab layout fix |
| `syncEditors()`               | Copies editor values into hidden textareas before form submit |

---

## Sequence Diagram

```
Browser                          Server (static files)
  │                                     │
  │  GET /templates/templates.json      │
  │────────────────────────────────────►│
  │◄────────────────────────────────────│  200 OK (JSON array)
  │                                     │
  │  [Render picker cards in DOM]       │
  │                                     │
  │  User clicks "Tin tức sản phẩm"    │
  │                                     │
  │  GET /templates/product-news.html   │
  │────────────────────────────────────►│
  │◄────────────────────────────────────│  200 OK (HTML text)
  │                                     │
  │  GET /templates/product-news.css    │
  │────────────────────────────────────►│
  │◄────────────────────────────────────│  200 OK (CSS text)
  │                                     │
  │  [No JS file in manifest → skip]   │
  │                                     │
  │  [Hide picker, show editors]        │
  │  [Create Monaco instances]          │
  │                                     │
  │  User edits content, clicks Save    │
  │                                     │
  │  POST /ContentFragments/Create      │
  │  (Name, HtmlContent, CssContent,    │
  │   JsContent, PageId)                │
  │────────────────────────────────────►│  Controller saves to DB
```
