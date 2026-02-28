using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Models;
using BravoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// db provider → "PostgreSQL" or "SQLite" from appsettings
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

// db init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (useSqlite)
        {
            // sqlite → drop & recreate (local cache, data comes from pg via pull)
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        else
        {
            // pg → apply migrations
            db.Database.Migrate();
        }

        // upsert template from wwwroot/templates/ files
        CustomTemplate SeedOrUpdateTemplate(
            string name, string icon, string description,
            string htmlFile, string? cssFile, string? jsFile)
        {
            var htmlContent = System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", htmlFile));
            var cssContent = cssFile != null ? System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", cssFile)) : null;
            var jsContent = jsFile != null ? System.IO.File.ReadAllText(Path.Combine(app.Environment.WebRootPath, "templates", jsFile)) : null;

            var template = db.CustomTemplates.FirstOrDefault(t => t.Name == name);
            if (template == null)
            {
                template = new CustomTemplate
                {
                    Name = name,
                    Icon = icon,
                    Description = description,
                    HtmlContent = htmlContent,
                    CssContent = cssContent,
                    JsContent = jsContent
                };
                db.CustomTemplates.Add(template);
            }
            else
            {
                template.Icon = icon;
                template.Description = description;
                template.HtmlContent = htmlContent;
                template.CssContent = cssContent;
                template.JsContent = jsContent;
            }
            db.SaveChanges();
            return template;
        }

        // seed templates
        var bannerTemplate = SeedOrUpdateTemplate(
            "Banner", "🖼️", "Banner toàn chiều rộng với tiêu đề, mô tả và nút CTA.",
            "banner.html", "banner.css", null);

        var productNewsTemplate = SeedOrUpdateTemplate(
            "Tin tức sản phẩm", "📰", "Khối tin tức dạng card 3 cột với tiêu đề, tag, và mô tả.",
            "product-news.html", "product-news.css", null);

        var testimonialsTemplate = SeedOrUpdateTemplate(
            "Phản hồi khách hàng", "💬", "Carousel phản hồi với 4 thẻ khách hàng.",
            "testimonials.html", "testimonials.css", "testimonials.js");

        var partnersTemplate = SeedOrUpdateTemplate(
            "Đối tác", "🤝", "Lưới logo đối tác 6x2 với inner borders.",
            "partners.html", "partners.css", null);

        // seed fragments
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
                DataJson = """{"PARTNER_1_IMAGE":"","PARTNER_1_URL":"#","PARTNER_2_IMAGE":"","PARTNER_2_URL":"#","PARTNER_3_IMAGE":"","PARTNER_3_URL":"#","PARTNER_4_IMAGE":"","PARTNER_4_URL":"#","PARTNER_5_IMAGE":"","PARTNER_5_URL":"#","PARTNER_6_IMAGE":"","PARTNER_6_URL":"#","PARTNER_7_IMAGE":"","PARTNER_7_URL":"#","PARTNER_8_IMAGE":"","PARTNER_8_URL":"#","PARTNER_9_IMAGE":"","PARTNER_9_URL":"#","PARTNER_10_IMAGE":"","PARTNER_10_URL":"#","PARTNER_11_IMAGE":"","PARTNER_11_URL":"#","PARTNER_12_IMAGE":"","PARTNER_12_URL":"#"}"""
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
app.MapRazorPages();

// mvc routes → higher priority
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// dynamic page catch-all → lower priority
app.MapControllerRoute(
    name: "dynamic-page",
    pattern: "{slug}",
    defaults: new { controller = "DynamicPage", action = "Show" });

app.Run();
