using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext sử dụng SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=tntea.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Cấu hình Cookie Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "TnTea.AuthCookie";
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization();

// 4. Đăng ký HttpContextAccessor để truy cập Session trong Razor Views
builder.Services.AddHttpContextAccessor();

// 5. Đăng ký MVC Controllers with Views và Razor Runtime Compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

// Tự động khởi tạo Database & Seed Data nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Đảm bảo bảng BrandStoryMilestones và CraftingSteps tồn tại
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS BrandStoryMilestones (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Year TEXT NOT NULL,
            Title TEXT NOT NULL,
            Description TEXT NOT NULL,
            ImageUrl TEXT NULL,
            OrderIndex INTEGER NOT NULL DEFAULT 0,
            IsActive INTEGER NOT NULL DEFAULT 1
        );
        CREATE TABLE IF NOT EXISTS CraftingSteps (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            StepNumber INTEGER NOT NULL,
            StepName TEXT NOT NULL,
            Description TEXT NOT NULL,
            IconCss TEXT NOT NULL DEFAULT 'bi bi-cup-hot',
            IsActive INTEGER NOT NULL DEFAULT 1
        );
        CREATE TABLE IF NOT EXISTS NewsArticles (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Slug TEXT NULL,
            Category TEXT NOT NULL,
            Badge TEXT NULL,
            Summary TEXT NOT NULL,
            Content TEXT NOT NULL,
            ImageUrl TEXT NOT NULL,
            PublishedAt TEXT NOT NULL,
            IsFeatured INTEGER NOT NULL DEFAULT 0,
            IsActive INTEGER NOT NULL DEFAULT 1,
            ViewCount INTEGER NOT NULL DEFAULT 0
        );
    ");

    // Đảm bảo cột Category tồn tại trong bảng Products
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Products ADD COLUMN Category TEXT NOT NULL DEFAULT 'Trà Sữa Đậm Hương';");
    }
    catch
    {
        // Cột đã tồn tại
    }

    // Tự động phân loại danh mục ban đầu cho các món sẵn có
    try
    {
        db.Database.ExecuteSqlRaw(@"
            UPDATE Products SET Category = 'Trà Sữa Truyền Thống' 
            WHERE (Category IS NULL OR Category = '' OR Category = 'Trà Sữa Đậm Hương') AND (Name LIKE '%Truyền Thống%' OR Name LIKE '%Đường Đen%' OR Name LIKE '%Assam%');

            UPDATE Products SET Category = 'Cheese & Matcha' 
            WHERE (Category IS NULL OR Category = '' OR Category = 'Trà Sữa Đậm Hương') AND (Name LIKE '%Cheese%' OR Name LIKE '%Matcha%' OR Name LIKE '%Kem%');

            UPDATE Products SET Category = 'Trà Trái Cây Tươi Mát' 
            WHERE (Category IS NULL OR Category = '' OR Category = 'Trà Sữa Đậm Hương') AND (Name LIKE '%Trái Cây%' OR Name LIKE '%Đào%' OR Name LIKE '%Dâu%' OR Name LIKE '%Vải%');
        ");
    }
    catch
    {
    }

    if (!db.BrandStoryMilestones.Any())
    {
        db.BrandStoryMilestones.AddRange(
            new BrandStoryMilestone
            {
                Year = "2020",
                Title = "Khởi Nguồn Đam Mê & Búp Trà Đầu Tiên",
                Description = "TN Tea được ấp ủ từ niềm say mê văn hóa thưởng trà thanh tao Á Đông. Cửa hàng thử nghiệm đầu tiên ra mắt với tôn chỉ trà mộc nguyên bản không phụ gia công nghiệp.",
                ImageUrl = "/images/ch1.jpg",
                OrderIndex = 1,
                IsActive = true
            },
            new BrandStoryMilestone
            {
                Year = "2022",
                Title = "Chọn Lọc Vùng Nguyên Liệu Thượng Hạng",
                Description = "Ký kết trực tiếp cùng các nông trường trà Oolong cao nguyên Mộc Châu và vùng ướp hoa tươi hữu cơ, bảo tồn trọn vẹn tinh dầu thanh khiết của búp trà non.",
                ImageUrl = "/images/ch2.jpg",
                OrderIndex = 2,
                IsActive = true
            },
            new BrandStoryMilestone
            {
                Year = "2024",
                Title = "Tiên Phong Công Thức Trà Sữa Tươi Lá",
                Description = "Tạm biệt bột sữa béo thông thường, TN Tea chuyển đổi hoàn toàn sang 100% sữa tươi thanh trùng Đà Lạt, đem đến vị ngọt êm dịu, nhẹ bụng và thơm mát tự nhiên.",
                ImageUrl = "/images/ch3.jpg",
                OrderIndex = 3,
                IsActive = true
            },
            new BrandStoryMilestone
            {
                Year = "2026",
                Title = "Vươn Xa & Phát Triển Hệ Thống Chuỗi Cửa Hàng",
                Description = "Phát triển không gian thưởng trà phong cách Sakura Pastel tinh tế tại các vị trí đắc địa, trở thành chốn dừng chân bình yên cho hàng triệu tín đồ trà sữa.",
                ImageUrl = "/images/ch4.jpg",
                OrderIndex = 4,
                IsActive = true
            }
        );
        db.SaveChanges();
    }

    if (!db.CraftingSteps.Any())
    {
        db.CraftingSteps.AddRange(
            new CraftingStep
            {
                StepNumber = 1,
                StepName = "Chọn Búp Trà Mộc",
                Description = "Thu hoạch thủ công búp trà Oolong và Assam 1 tôm 2 lá tươi non lúc sáng sớm tại đồn điền trà cao nguyên mát lành.",
                IconCss = "bi bi-tree",
                IsActive = true
            },
            new CraftingStep
            {
                StepNumber = 2,
                StepName = "Ướp Hương Hoa Tự Nhiên",
                Description = "Ủ mộc cùng cánh hoa anh đào và hoa nhài thanh khiết suốt 24 giờ để từng giọt tinh dầu hoa thấm sâu vào sớ trà.",
                IconCss = "bi bi-flower1",
                IsActive = true
            },
            new CraftingStep
            {
                StepNumber = 3,
                StepName = "Nấu Trân Châu Thủ Công",
                Description = "Từng mẻ trân châu hoàng kim được nấu mới mỗi 4 giờ, ngào đượm mật mía đường đen tự nhiên cho độ dẻo giòn chuẩn mực.",
                IconCss = "bi bi-fire",
                IsActive = true
            },
            new CraftingStep
            {
                StepNumber = 4,
                StepName = "Trao Tay Trọn Vị",
                Description = "Lắc đều chuẩn định lượng cùng sữa tươi thanh trùng và trao gửi đến tay bạn ly trà sữa tươi mát, đánh thức mọi giác quan.",
                IconCss = "bi bi-cup-straw",
                IsActive = true
            }
        );
        db.SaveChanges();
    }

    if (!db.NewsArticles.Any())
    {
        db.NewsArticles.AddRange(
            new NewsArticle
            {
                Title = "Mùa Hoa Sakura: Mua 1 Tặng 1 Toàn Bộ Menu Trà Sữa TN Tea",
                Slug = "mua-hoa-sakura-mua-1-tang-1-toan-bo-menu-tra-sua-tn-tea",
                Category = "Sự Kiện",
                Badge = "HOT EVENT",
                PublishedAt = new DateTime(2026, 10, 18, 9, 0, 0),
                ImageUrl = "/baner.jpg",
                Summary = "Chào đón mùa lễ hội hoa anh đào rực rỡ, TN Tea gửi tặng bạn ưu đãi nhân đôi niềm vui: Mua 1 ly bất kỳ tặng ngay 1 ly Trà Sữa Trân Châu Hoàng Gia hoặc Trà Đào Cam Sả mát lạnh. Áp dụng cho tất cả khách hàng đặt hàng tại quầy và online!",
                Content = "Mùa hoa anh đào Sakura đã chính thức gõ cửa TN Tea! Để tri ân sự đồng hành của quý khách hàng, TN Tea mang đến chương trình ưu đãi lớn nhất trong năm: MUA 1 TẶNG 1 TOÀN BỘ MENU TRÀ SỮA.\n\nKhi mua bất kỳ 1 ly trà sữa size L, bạn sẽ được tặng ngay 1 ly Trà Sữa Trân Châu Hoàng Gia hoặc Trà Sữa Truyền Thống size M hoàn toàn miễn phí. Chương trình áp dụng trên toàn hệ thống cửa hàng và đặt món trực tuyến qua website.",
                IsFeatured = true,
                IsActive = true,
                ViewCount = 1250
            },
            new NewsArticle
            {
                Title = "Bí Quyết Tạo Nên Lớp Cheese Béo Ngậy Của TN Tea",
                Slug = "bi-quyet-tao-nen-lop-cheese-beo-ngay-cua-tn-tea",
                Category = "Ẩm Thực",
                Badge = "Ẩm Thực",
                PublishedAt = new DateTime(2026, 10, 15, 8, 30, 0),
                ImageUrl = "https://images.unsplash.com/photo-1576092768241-dec231879fc3?w=600&auto=format&fit=crop&q=80",
                Summary = "Tìm hiểu công thức kem cheese phô mai béo mịn độc quyền giúp tôn trọn vị trà Oolong sao nướng đậm đà.",
                Content = "Lớp kem Cheese béo ngậy sánh mịn tại TN Tea được đánh bông thủ công mỗi sáng từ kem sữa Anchor New Zealand và phô mai cream cheese nguyên chất. Sự kết hợp hoàn hảo giữa vị mằn mặn dịu êm của phô mai và hậu vị thanh ngọt của trà Oolong nướng tạo nên trải nghiệm khó quên.",
                IsFeatured = false,
                IsActive = true,
                ViewCount = 890
            },
            new NewsArticle
            {
                Title = "Hành Trình Mang Vị Bột Matcha Uji Kyoto Về Việt Nam",
                Slug = "hanh-trinh-mang-vi-bot-matcha-uji-kyoto-ve-viet-nam",
                Category = "Câu Chuyện",
                Badge = "Câu Chuyện",
                PublishedAt = new DateTime(2026, 10, 10, 14, 0, 0),
                ImageUrl = "https://images.unsplash.com/photo-1536256263959-770b48d82b0a?w=600&auto=format&fit=crop&q=80",
                Summary = "Khám phá quy trình tuyển chọn những lá trà xanh non mướt trên cao nguyên Kyoto cổ kính để làm nên ly Matcha TN Tea.",
                Content = "Kyoto - thủ phủ của những đồn điền trà xanh lâu đời nhất Nhật Bản, là nơi TN Tea tìm kiếm hương vị bột Matcha thuần khiết. Từng búp trà non được che râm 20 ngày trước thu hoạch để tích tụ diệp lục tố cao nhất, sau đó nghiền chậm bằng cối đá granite truyền thống.",
                IsFeatured = false,
                IsActive = true,
                ViewCount = 670
            },
            new NewsArticle
            {
                Title = "Top 3 Vị Trà Thanh Lọc Giải Nhiệt Được Yêu Thích Nhất",
                Slug = "top-3-vi-tra-thanh-loc-giai-nhiet-duoc-yeu-thich-nhat",
                Category = "Sức Khỏe",
                Badge = "Sức Khỏe",
                PublishedAt = new DateTime(2026, 10, 5, 10, 15, 0),
                ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=600&auto=format&fit=crop&q=80",
                Summary = "Giải nhiệt ngày hè sảng khoái cùng các dòng trà hoa quả giàu Vitamin C tự nhiên giúp tinh thần tươi mới.",
                Content = "Giữa những ngày oi ả, một ly trà hoa quả mát lạnh không chỉ giải khát tức thì mà còn bổ sung lượng vitamin dồi dào cho cơ thể. Cùng TN Tea điểm qua 3 công thức trà thanh mát gồm Trà Đào Cam Sả, Trà Lài Chanh Dây và Trà Sen Vàng Thanh Khiết.",
                IsFeatured = false,
                IsActive = true,
                ViewCount = 1120
            }
        );
        db.SaveChanges();
    }
}

// Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Bắt buộc đặt UseSession sau UseRouting và trước UseAuthentication/UseAuthorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Route mặc định dẫn trực tiếp vào danh sách sản phẩm TN Tea
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
