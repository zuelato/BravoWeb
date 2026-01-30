using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed initial fragments
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();

        if (!db.ContentFragments.Any(f => f.Name == "banner"))
        {
            var banner = """
<div class="banner-container" style="background-image: url('https://thumbs2.imgbox.com/c5/cf/nBfprcbk_t.jpg')">
    <div class="banner-container-content">
        <div class="banner-container-content-title">
            <p>Gi?i pháp ph?n m?m qu?n tr? doanh nghi?p BRAVO ERP</p>
        </div>
        <div class="banner-container-content-subtitle">
            <p>
                Gi?i pháp ph?n m?m qu?n lý doanh nghi?p BRAVO là s? k?t h?p hoàn h?o gi?a s?n ph?m "ph?n m?m" v?i nh?ng
                "kinh nghi?m t? v?n và tri?n khai ph?n m?m" s? tr? thành "Bí quy?t qu?n tr? doanh nghi?p" c?a các doanh nghi?p.
            </p>
        </div>
        <div class="banner-container-content-about-us">
            <a class="cta-link" href="#">
                <div class="banner-container-content-button-container">
                    <p>V? CHÚNG TÔI</p>
                </div>
            </a>
        </div>
    </div>
</div>
""";
            db.ContentFragments.Add(new ContentFragment { Name = "banner", HtmlContent = banner, DisplayOrder = 0 });
        }

        if (!db.ContentFragments.Any(f => f.Name == "product_news"))
        {
            var productNews = """
<div class="product_news-section-container">
    <div class="product_news-content">
        <div class="upper-section-content">
            <div class="section-name">
                <div class="name">
                    <p>PRODUCT NEWS</p>
                </div>
                <div class="divider">
                    <div class="divider-left"></div>
                    <div class="divider-right"></div>
                </div>
            </div>

            <div class="section-title">
                <div class="title">
                    <p>Tin t?c v? s?n ph?m</p>
                </div>

                <a href="/" class="see-all-button">
                    Xem t?t c?
                </a>
            </div>
        </div>

        <div class="product_news-lower-section-content">
            <a href="/" class="product_news-news-card">
                <div class="image"></div>
                <div class="product_news-news-block">
                    <div class="product_news-news-tag">&news-tag</div>
                    <div class="product_news-news-title">&news-title</div>
                    <div class="product_news-news-desc">&news-desc</div>
                </div>
            </a>

            <a href="/" class="product_news-news-card">
                <div class="image"></div>
                <div class="product_news-news-block">
                    <div class="product_news-news-tag">&news-tag</div>
                    <div class="product_news-news-title">&news-title</div>
                    <div class="product_news-news-desc">&news-desc</div>
                </div>
            </a>

            <a href="/" class="product_news-news-card">
                <div class="image"></div>
                <div class="product_news-news-block">
                    <div class="product_news-news-tag">&news-tag</div>
                    <div class="product_news-news-title">&news-title</div>
                    <div class="product_news-news-desc">&news-desc</div>
                </div>
            </a>
        </div>
    </div>
</div>
""";
            db.ContentFragments.Add(new ContentFragment { Name = "product_news", HtmlContent = productNews, DisplayOrder = 1 });
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
