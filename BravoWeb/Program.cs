using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BravoWeb.Data;
using BravoWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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
            // SQLite: create schema from model (no migrations needed for local dev)
            db.Database.EnsureCreated();
        }
        else
        {
            // PostgreSQL: apply pending migrations
            db.Database.Migrate();
        }

        // Seed initial fragments
        if (!db.ContentFragments.Any(f => f.Name == "banner"))
        {
            var banner = """
<div class="banner-container">
    <div class="banner-container-content">
        <div class="banner-container-content-title">
            <p>Giải pháp phần mềm quản trị doanh nghiệp BRAVO ERP</p>
        </div>
        <div class="banner-container-content-subtitle">
            <p>
                Giải pháp phần mềm quản lý doanh nghiệp BRAVO là sự kết hợp hoàn hảo giữa sản phẩm "phần mềm" với những
                "kinh nghiệm tư vấn và triển khai phần mềm" sẽ trở thành "Bí quyết quản trị doanh nghiệp" của các doanh nghiệp.
            </p>
        </div>
        <div class="banner-container-content-about-us">
            <a class="cta-link" href="#">
                <div class="banner-container-content-button-container">
                    <p>VỀ CHÚNG TÔI</p>
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
                    <p>Tin tức về sản phẩm</p>
                </div>

                <a href="/" class="see-all-button">
                    Xem tất cả
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

        // Seed testimonials fragment
        if (!db.ContentFragments.Any(f => f.Name == "testimonials"))
        {
            var testimonials = """
<div class="section-container">
    <div class="content">
        <div class="upper-section-content">
            <div class="section-name">
                <div class="name">
                    <p>TESTIMONIALS</p>
                </div>
                <div class="divider">
                    <div class="divider-left"></div>
                    <div class="divider-right"></div>
                </div>
            </div>

            <div class="section-title">
                <div class="title">
                    <p>Phản hồi từ khách hàng</p>
                </div>
            </div>
        </div>
        <div class="lower-section-content">
            <div class="carousel-container" id="testimonials-carousel">
                <div class="carousel-track">
                    <div class="carousel-card">
                        <div class="carousel-card-upper-section">
                            <div class="carousel-card-upper-section-portrait"></div>
                            <div class="carousel-card-upper-section-title">
                                <div class="testimonial-carousel-name">Nguyễn Văn A</div>
                                <div class="testimonial-carousel-title">Giám đốc</div>
                            </div>
                        </div>
                        <div class="carousel-card-lower-section">
                            Bravo đã giúp chúng tôi tối ưu quy trình và tăng trưởng hiệu quả.
                        </div>
                    </div>

                    <div class="carousel-card">
                        <div class="carousel-card-upper-section">
                            <div class="carousel-card-upper-section-portrait"></div>
                            <div class="carousel-card-upper-section-title">
                                <div class="testimonial-carousel-name">Trần Thị B</div>
                                <div class="testimonial-carousel-title">Trưởng phòng</div>
                            </div>
                        </div>
                        <div class="carousel-card-lower-section">
                            Dịch vụ chuyên nghiệp và đội ngũ tận tâm.
                        </div>
                    </div>

                    <div class="carousel-card">
                        <div class="carousel-card-upper-section">
                            <div class="carousel-card-upper-section-portrait"></div>
                            <div class="carousel-card-upper-section-title">
                                <div class="testimonial-carousel-name">Lê Văn C</div>
                                <div class="testimonial-carousel-title">Chủ doanh nghiệp</div>
                            </div>
                        </div>
                        <div class="carousel-card-lower-section">
                            Hệ thống ổn định, báo cáo chính xác, rất hài lòng.
                        </div>
                    </div>

                    <div class="carousel-card">
                        <div class="carousel-card-upper-section">
                            <div class="carousel-card-upper-section-portrait"></div>
                            <div class="carousel-card-upper-section-title">
                                <div class="testimonial-carousel-name">Phạm Thị D</div>
                                <div class="testimonial-carousel-title">Kế toán trưởng</div>
                            </div>
                        </div>
                        <div class="carousel-card-lower-section">
                            Phần mềm dễ sử dụng, tiết kiệm thời gian cho bộ phận kế toán.
                        </div>
                    </div>
                </div>
            </div>

            <div class="carousel-controller">
                <div class="carousel-controller-prev-nav" id="carousel-prev">
                    <p>&lt;</p>
                </div>

                <div class="carousel-controller-next-nav" id="carousel-next">
                    <p>&gt;</p>
                </div>
            </div>
        </div>
    </div>
</div>

<script>
    (function(){
        const container = document.getElementById('testimonials-carousel');
        const track = container.querySelector('.carousel-track');
        const prevBtn = document.getElementById('carousel-prev');
        const nextBtn = document.getElementById('carousel-next');
        let cards = Array.from(track.children);
        let cardWidth = 0;
        let gapBetween = 0;
        let halfGap = 0;
        let currentTranslate = 0;
        let isAnimating = false;

        function recalcSizesOnly(){
            const viewport = window.innerWidth || document.documentElement.clientWidth || container.clientWidth;

            const computed = getComputedStyle(cards[0]);
            const ml = parseFloat(computed.marginLeft) || 0;
            const mr = parseFloat(computed.marginRight) || 0;
            gapBetween = ml + mr;
            halfGap = ml;

            cardWidth = (viewport - 3 * gapBetween) / 3;
            cards.forEach(c => { c.style.flex = `0 0 ${cardWidth}px`; });
        }

        function restingOffset(){
            return -(cardWidth / 2 + halfGap);
        }

        function updateSizes(){
            recalcSizesOnly();
            currentTranslate = restingOffset();
            setTranslate(currentTranslate, false);
        }

        function setTranslate(x, animate=true){
            track.style.transition = animate ? 'transform 0.45s ease' : 'none';
            track.style.transform = `translate3d(${x}px,0,0)`;
        }

        function moveNext(){
            if(isAnimating) return;
            isAnimating = true;
            nextBtn.style.pointerEvents = 'none';
            prevBtn.style.pointerEvents = 'none';
            const step = cardWidth + gapBetween;
            const target = currentTranslate - step;
            setTranslate(target, true);
            const onEnd = () => {
                track.removeEventListener('transitionend', onEnd);
                track.appendChild(track.firstElementChild);
                cards = Array.from(track.children);
                recalcSizesOnly();
                void track.offsetWidth;
                currentTranslate = restingOffset();
                setTranslate(currentTranslate, false);
                isAnimating = false;
                nextBtn.style.pointerEvents = '';
                prevBtn.style.pointerEvents = '';
            };
            track.addEventListener('transitionend', onEnd);
        }

        function movePrev(){
            if(isAnimating) return;
            isAnimating = true;
            nextBtn.style.pointerEvents = 'none';
            prevBtn.style.pointerEvents = 'none';
            const step = cardWidth + gapBetween;
            track.insertBefore(track.lastElementChild, track.firstChild);
            cards = Array.from(track.children);
            recalcSizesOnly();
            currentTranslate = restingOffset() - step;
            setTranslate(currentTranslate, false);
            void track.offsetWidth;
            const target = restingOffset();
            setTranslate(target, true);
            const onEnd = () => {
                track.removeEventListener('transitionend', onEnd);
                currentTranslate = target;
                cards = Array.from(track.children);
                isAnimating = false;
                nextBtn.style.pointerEvents = '';
                prevBtn.style.pointerEvents = '';
            };
            track.addEventListener('transitionend', onEnd);
        }

        nextBtn.addEventListener('click', moveNext);
        prevBtn.addEventListener('click', movePrev);
        window.addEventListener('resize', updateSizes);
        updateSizes();
    })();
</script>
""";

            db.ContentFragments.Add(new ContentFragment { Name = "testimonials", HtmlContent = testimonials, DisplayOrder = 2 });
        }

        // Seed partners fragment
        if (!db.ContentFragments.Any(f => f.Name == "partners"))
        {
            var partners = """
<div class="partners-section">
    <div class="content">
        <div class="partners-viewport">
            <div class="partners-track">
                <div class="partner-grid">
                    <div class="partner-cell">&client-logo</div>
                    <div class="partner-cell">&client-logo</div>
                    <div class="partner-cell">&client-logo</div>
                    <div class="partner-cell">&client-logo</div>
                    <div class="partner-cell">&client-logo</div>
                    <div class="partner-cell">&client-logo</div>
                    <div class="partner-cell">&client-logo</div>
                    <div class="partner-cell">&client-logo</div>
                </div>
            </div>
        </div>
    </div>
</div>
""";

            db.ContentFragments.Add(new ContentFragment { Name = "partners", HtmlContent = partners, DisplayOrder = 3 });
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
