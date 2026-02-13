using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Models;
using BravoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<TemplateRenderer>();

// ── Database provider selection ──
// Reads "DatabaseProvider" from appsettings (overridden per environment).
// "PostgreSQL" → uses DefaultConnection with Npgsql
// "SQLite"     → uses SqliteConnection with local file DB
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
var useSqlite = dbProvider.Equals("SQLite", StringComparison.OrdinalIgnoreCase);

if (useSqlite)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null)));
}

var app = builder.Build();

// ── Database initialisation ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (useSqlite)
        {
            // SQLite: drop and recreate so schema always matches the current model.
            // SQLite is a local cache — data comes from PostgreSQL via Pull.
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        else
        {
            // PostgreSQL: apply pending migrations
            db.Database.Migrate();
        }

        // ── Seed banner template ──
        var bannerTemplate = db.CustomTemplates.FirstOrDefault(t => t.Name == "Banner");
        if (bannerTemplate == null)
        {
            bannerTemplate = new CustomTemplate
            {
                Name = "Banner",
                Icon = "🖼️",
                Description = "Banner toàn chiều rộng với tiêu đề, mô tả và nút CTA.",
                HtmlContent = """
<div class="banner-container" style="background-image: url('{{BG_IMAGE}}');">
    <div class="banner-container-content">
        <div class="banner-container-content-title">
            <p>{{TITLE}}</p>
        </div>
        <div class="banner-container-content-subtitle">
            <p>{{SUBTITLE}}</p>
        </div>
        <div class="banner-container-content-about-us">
            <a class="cta-link" href="{{CTA_HREF}}">
                <div class="banner-container-content-button-container">
                    <p>{{CTA_TEXT}}</p>
                </div>
            </a>
        </div>
    </div>
</div>
""",
                CssContent = """
.banner-container {
    position: relative;
    left: 50%;
    right: 50%;
    margin-left: -50vw;
    margin-right: -50vw;
    width: 100vw;
    min-height: 60vh;
    z-index: 0;
    background-position: center;
    background-repeat: no-repeat;
    background-size: cover;
    background-color: #003366;
    overflow: hidden;
}

.banner-container a {
    text-decoration: none;
}

.banner-container-content {
    display: flex;
    flex-direction: column;
    gap: 20px;
    position: relative;
    z-index: 1;
    width: clamp(300px, 60ch, 50%);
    align-items: flex-start;
    text-align: left;
    margin-left: var(--site-side-margin, 20%);
    padding: 6vh 0;
}

.banner-container-content p {
    color: white;
}

.banner-container-content-title {
    font-size: 28px;
    font-weight: bold;
    color: white;
    margin-bottom: 20px;
}

.banner-container-content-subtitle {
    font-size: 18px;
    color: white;
}

.banner-container-content-about-us {
    width: 40%;
    padding-top: 10px;
    padding-bottom: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f8ae49;
    color: white;
    border-radius: 10px;
    margin-top: 15px;
}

.banner-container-content-button-container {
    display: flex;
    justify-content: center;
    align-items: center;
    text-align: center;
}

.banner-container-content-about-us p {
    font-size: 18px;
}

.banner-container-content-about-us:hover {
    background-color: #00a88e;
}
"""
            };
            db.CustomTemplates.Add(bannerTemplate);
            db.SaveChanges();
        }

        // ── Seed product news template ──
        var productNewsTemplate = db.CustomTemplates.FirstOrDefault(t => t.Name == "Tin tức sản phẩm");
        if (productNewsTemplate == null)
        {
            productNewsTemplate = new CustomTemplate
            {
                Name = "Tin tức sản phẩm",
                Icon = "📰",
                Description = "Khối tin tức dạng card 3 cột với tiêu đề, tag, và mô tả.",
                HtmlContent = """
<div class="product_news-section-container">
    <div class="product_news-content">
        <div class="upper-section-content">
            <div class="section-name">
                <div class="name">
                    <p>{{SECTION_LABEL}}</p>
                </div>
                <div class="divider">
                    <div class="divider-left"></div>
                    <div class="divider-right"></div>
                </div>
            </div>
            <div class="section-title">
                <div class="title">
                    <p>{{SECTION_TITLE}}</p>
                </div>
                <a href="{{SEE_ALL_URL}}" class="see-all-button">Xem tất cả</a>
            </div>
        </div>
        <div class="product_news-lower-section-content">
            <a href="{{CARD_1_URL}}" class="product_news-news-card">
                <div class="image" style="background-image: url('{{CARD_1_IMAGE}}');"></div>
                <div class="product_news-news-block">
                    <div class="product_news-news-tag">{{CARD_1_TAG}}</div>
                    <div class="product_news-news-title">{{CARD_1_TITLE}}</div>
                    <div class="product_news-news-desc">{{CARD_1_DESC}}</div>
                </div>
            </a>
            <a href="{{CARD_2_URL}}" class="product_news-news-card">
                <div class="image" style="background-image: url('{{CARD_2_IMAGE}}');"></div>
                <div class="product_news-news-block">
                    <div class="product_news-news-tag">{{CARD_2_TAG}}</div>
                    <div class="product_news-news-title">{{CARD_2_TITLE}}</div>
                    <div class="product_news-news-desc">{{CARD_2_DESC}}</div>
                </div>
            </a>
            <a href="{{CARD_3_URL}}" class="product_news-news-card">
                <div class="image" style="background-image: url('{{CARD_3_IMAGE}}');"></div>
                <div class="product_news-news-block">
                    <div class="product_news-news-tag">{{CARD_3_TAG}}</div>
                    <div class="product_news-news-title">{{CARD_3_TITLE}}</div>
                    <div class="product_news-news-desc">{{CARD_3_DESC}}</div>
                </div>
            </a>
        </div>
    </div>
</div>
""",
                CssContent = System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", "product-news.css"))
            };
            db.CustomTemplates.Add(productNewsTemplate);
            db.SaveChanges();
        }

        // ── Seed testimonials template ──
        var testimonialsTemplate = db.CustomTemplates.FirstOrDefault(t => t.Name == "Phản hồi khách hàng");
        if (testimonialsTemplate == null)
        {
            testimonialsTemplate = new CustomTemplate
            {
                Name = "Phản hồi khách hàng",
                Icon = "💬",
                Description = "Carousel phản hồi với 4 thẻ khách hàng.",
                HtmlContent = System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", "testimonials.html")),
                CssContent = System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", "testimonials.css")),
                JsContent = System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", "testimonials.js"))
            };
            db.CustomTemplates.Add(testimonialsTemplate);
            db.SaveChanges();
        }

        // ── Seed partners template ──
        var partnersTemplate = db.CustomTemplates.FirstOrDefault(t => t.Name == "Đối tác");
        if (partnersTemplate == null)
        {
            partnersTemplate = new CustomTemplate
            {
                Name = "Đối tác",
                Icon = "🤝",
                Description = "Lưới logo đối tác 4x2.",
                HtmlContent = System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", "partners.html")),
                CssContent = System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", "partners.css"))
            };
            db.CustomTemplates.Add(partnersTemplate);
            db.SaveChanges();
        }

        // ── Seed fragments ──
        if (!db.ContentFragments.Any(f => f.Name == "banner"))
        {
            db.ContentFragments.Add(new ContentFragment
            {
                Name = "banner",
                HtmlContent = "<!-- rendered from template -->",
                DisplayOrder = 0,
                TemplateId = bannerTemplate.Id,
                DataJson = """{"TITLE":"Giải pháp phần mềm quản trị doanh nghiệp BRAVO ERP","SUBTITLE":"Giải pháp phần mềm quản lý doanh nghiệp BRAVO là sự kết hợp hoàn hảo giữa sản phẩm \"phần mềm\" với những \"kinh nghiệm tư vấn và triển khai phần mềm\" sẽ trở thành \"Bí quyết quản trị doanh nghiệp\" của các doanh nghiệp.","BG_IMAGE":"https://thumbs2.imgbox.com/c5/cf/nBfprcbk_t.jpg","CTA_TEXT":"VỀ CHÚNG TÔI","CTA_HREF":"#"}"""
            });
        }

        if (!db.ContentFragments.Any(f => f.Name == "product_news"))
        {
            db.ContentFragments.Add(new ContentFragment
            {
                Name = "product_news",
                HtmlContent = "<!-- rendered from template -->",
                DisplayOrder = 1,
                TemplateId = productNewsTemplate.Id,
                DataJson = """{"SECTION_LABEL":"PRODUCT NEWS","SECTION_TITLE":"Tin tức về sản phẩm","SEE_ALL_URL":"/","CARD_1_URL":"/","CARD_1_IMAGE":"","CARD_1_TAG":"Tag 1","CARD_1_TITLE":"Tiêu đề 1","CARD_1_DESC":"Mô tả 1","CARD_2_URL":"/","CARD_2_IMAGE":"","CARD_2_TAG":"Tag 2","CARD_2_TITLE":"Tiêu đề 2","CARD_2_DESC":"Mô tả 2","CARD_3_URL":"/","CARD_3_IMAGE":"","CARD_3_TAG":"Tag 3","CARD_3_TITLE":"Tiêu đề 3","CARD_3_DESC":"Mô tả 3"}"""
            });
        }

        if (!db.ContentFragments.Any(f => f.Name == "testimonials"))
        {
            db.ContentFragments.Add(new ContentFragment
            {
                Name = "testimonials",
                HtmlContent = "<!-- rendered from template -->",
                DisplayOrder = 2,
                TemplateId = testimonialsTemplate.Id,
                DataJson = """{"SECTION_LABEL":"TESTIMONIALS","SECTION_TITLE":"Phản hồi từ khách hàng","CARD_1_NAME":"Nguyễn Văn A","CARD_1_TITLE":"Giám đốc","CARD_1_QUOTE":"Bravo đã giúp chúng tôi tối ưu quy trình và tăng trưởng hiệu quả.","CARD_2_NAME":"Trần Thị B","CARD_2_TITLE":"Trưởng phòng","CARD_2_QUOTE":"Dịch vụ chuyên nghiệp và đội ngũ tận tâm.","CARD_3_NAME":"Lê Văn C","CARD_3_TITLE":"Chủ doanh nghiệp","CARD_3_QUOTE":"Hệ thống ổn định, báo cáo chính xác, rất hài lòng.","CARD_4_NAME":"Phạm Thị D","CARD_4_TITLE":"Kế toán trưởng","CARD_4_QUOTE":"Phần mềm dễ sử dụng, tiết kiệm thời gian cho bộ phận kế toán."}"""
            });
        }

        if (!db.ContentFragments.Any(f => f.Name == "partners"))
        {
            db.ContentFragments.Add(new ContentFragment
            {
                Name = "partners",
                HtmlContent = "<!-- rendered from template -->",
                DisplayOrder = 3,
                TemplateId = partnersTemplate.Id,
                DataJson = """{"PARTNER_1":"Logo 1","PARTNER_2":"Logo 2","PARTNER_3":"Logo 3","PARTNER_4":"Logo 4","PARTNER_5":"Logo 5","PARTNER_6":"Logo 6","PARTNER_7":"Logo 7","PARTNER_8":"Logo 8"}"""
            });
        }

        db.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Map Razor Pages so admin pages are reachable
app.MapRazorPages();

// Default MVC routes (higher priority — matched first)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Dynamic page catch-all (lower priority — only matched if no MVC controller handled it)
app.MapControllerRoute(
    name: "dynamic-page",
    pattern: "{slug}",
    defaults: new { controller = "DynamicPage", action = "Show" });

app.Run();
