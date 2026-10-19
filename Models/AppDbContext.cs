using Microsoft.EntityFrameworkCore;

namespace WebTraSua.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    public DbSet<BrandStoryMilestone> BrandStoryMilestones { get; set; } = null!;
    public DbSet<CraftingStep> CraftingSteps { get; set; } = null!;
    public DbSet<NewsArticle> NewsArticles { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình bảng User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Cấu hình quan hệ Order -> User (1 - N)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình quan hệ OrderDetail -> Order (N - 1)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cấu hình quan hệ OrderDetail -> Product (N - 1)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Khởi tạo Seed Data cho User
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "Quản Trị Viên TN Tea",
                Email = "admin@tntea.vn",
                PhoneNumber = "0901234567",
                PasswordHash = "admin123",
                Role = "Admin",
                CreatedAt = new DateTime(2026, 1, 1),
                Username = "admin",
                Password = "123"
            },
            new User
            {
                Id = 2,
                FullName = "Khách Hàng Thân Thiết",
                Email = "khachhang@gmail.com",
                PhoneNumber = "0987654321",
                PasswordHash = "123456",
                Role = "Customer",
                CreatedAt = new DateTime(2026, 1, 1),
                Username = "khachhang",
                Password = "123"
            }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductId = 1,
                Name = "Trà Sữa Trân Châu Đường Đen Hoàng Gia",
                Price = 35000,
                Description = "Hương vị trà đen Assam thượng hạng hòa quyện cùng sữa tươi thanh trùng và từng hạt trân châu hoàng kim dai giòn ngào đường đen mật mía thơm nồng.",
                ImageUrl = "https://images.unsplash.com/photo-1558857563-b37df35d1f86?w=600&auto=format&fit=crop&q=80"
            },
            new Product
            {
                ProductId = 2,
                Name = "Trà Sữa Matcha Đậu Đỏ Nhật Bản",
                Price = 40000,
                Description = "Bột Matcha Uji nguyên chất nhập khẩu từ Kyoto kết hợp vị sữa béo ngậy thanh nhẹ, phủ lớp đậu đỏ hầm mềm ngọt bùi tự nhiên.",
                ImageUrl = "https://images.unsplash.com/photo-1536256263959-770b48d82b0a?w=600&auto=format&fit=crop&q=80"
            },
            new Product
            {
                ProductId = 3,
                Name = "Trà Sữa Truyền Thống Đậm Vị TN Tea",
                Price = 38000,
                Description = "Vị trà đen mộc truyền thống hòa quyện cùng sữa tươi thanh trùng Đà Lạt và trân châu hoàng kim chuẩn vị thơm béo đậm đà.",
                ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=600&auto=format&fit=crop&q=80"
            },
            new Product
            {
                ProductId = 4,
                Name = "Trà Sữa Oolong Nướng Cheese Kem Trứng",
                Price = 45000,
                Description = "Cốt trà Oolong Đài Loan được sao nướng kỹ lưỡng mang hương khói đặc trưng, kết hợp lớp Macchiato phô mai kem trứng béo ngậy sánh mịn thơm lừng.",
                ImageUrl = "https://images.unsplash.com/photo-1576092768241-dec231879fc3?w=600&auto=format&fit=crop&q=80"
            }
        );

        // Cấu hình Fluent API cho BrandStoryMilestone
        modelBuilder.Entity<BrandStoryMilestone>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Year).IsRequired().HasMaxLength(10);
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).IsRequired().HasMaxLength(1000);
            entity.Property(m => m.OrderIndex).HasDefaultValue(0);
            entity.Property(m => m.IsActive).HasDefaultValue(true);
        });

        // Seed Data: 4 mốc Hành Trình Hương Vị
        modelBuilder.Entity<BrandStoryMilestone>().HasData(
            new BrandStoryMilestone
            {
                Id = 1,
                Year = "2020",
                Title = "Khởi Nguồn Đam Mê & Búp Trà Đầu Tiên",
                Description = "TN Tea được ấp ủ từ niềm say mê văn hóa thưởng trà thanh tao Á Đông. Cửa hàng thử nghiệm đầu tiên ra mắt với tôn chỉ trà mộc nguyên bản không phụ gia công nghiệp.",
                ImageUrl = "/images/ch1.jpg",
                OrderIndex = 1,
                IsActive = true
            },
            new BrandStoryMilestone
            {
                Id = 2,
                Year = "2022",
                Title = "Chọn Lọc Vùng Nguyên Liệu Thượng Hạng",
                Description = "Ký kết trực tiếp cùng các nông trường trà Oolong cao nguyên Mộc Châu và vùng ướp hoa tươi hữu cơ, bảo tồn trọn vẹn tinh dầu thanh khiết của búp trà non.",
                ImageUrl = "/images/ch2.jpg",
                OrderIndex = 2,
                IsActive = true
            },
            new BrandStoryMilestone
            {
                Id = 3,
                Year = "2024",
                Title = "Tiên Phong Công Thức Trà Sữa Tươi Lá",
                Description = "Tạm biệt bột sữa béo thông thường, TN Tea chuyển đổi hoàn toàn sang 100% sữa tươi thanh trùng Đà Lạt, đem đến vị ngọt êm dịu, nhẹ bụng và thơm mát tự nhiên.",
                ImageUrl = "/images/ch3.jpg",
                OrderIndex = 3,
                IsActive = true
            },
            new BrandStoryMilestone
            {
                Id = 4,
                Year = "2026",
                Title = "Vươn Xa & Phát Triển Hệ Thống Chuỗi Cửa Hàng",
                Description = "Phát triển không gian thưởng trà phong cách Sakura Pastel tinh tế tại các vị trí đắc địa, trở thành chốn dừng chân bình yên cho hàng triệu tín đồ trà sữa.",
                ImageUrl = "/images/ch4.jpg",
                OrderIndex = 4,
                IsActive = true
            }
        );

        // Cấu hình Fluent API cho CraftingStep
        modelBuilder.Entity<CraftingStep>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.StepNumber).IsRequired();
            entity.Property(s => s.StepName).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Description).IsRequired().HasMaxLength(1000);
            entity.Property(s => s.IconCss).HasMaxLength(100).HasDefaultValue("bi bi-cup-hot");
            entity.Property(s => s.IsActive).HasDefaultValue(true);
        });

        // Seed Data: 4 bước Quy Trình Tuyển Chọn & Pha Chế (Farm to Cup)
        modelBuilder.Entity<CraftingStep>().HasData(
            new CraftingStep
            {
                Id = 1,
                StepNumber = 1,
                StepName = "Chọn Búp Trà Mộc",
                Description = "Thu hoạch thủ công búp trà Oolong và Assam 1 tôm 2 lá tươi non lúc sáng sớm tại đồn điền trà cao nguyên mát lành.",
                IconCss = "bi bi-tree",
                IsActive = true
            },
            new CraftingStep
            {
                Id = 2,
                StepNumber = 2,
                StepName = "Ướp Hương Hoa Tự Nhiên",
                Description = "Ủ mộc cùng cánh hoa anh đào và hoa nhài thanh khiết suốt 24 giờ để từng giọt tinh dầu hoa thấm sâu vào sớ trà.",
                IconCss = "bi bi-flower1",
                IsActive = true
            },
            new CraftingStep
            {
                Id = 3,
                StepNumber = 3,
                StepName = "Nấu Trân Châu Thủ Công",
                Description = "Từng mẻ trân châu hoàng kim được nấu mới mỗi 4 giờ, ngào đượm mật mía đường đen tự nhiên cho độ dẻo giòn chuẩn mực.",
                IconCss = "bi bi-fire",
                IsActive = true
            },
            new CraftingStep
            {
                Id = 4,
                StepNumber = 4,
                StepName = "Trao Tay Trọn Vị",
                Description = "Lắc đều chuẩn định lượng cùng sữa tươi thanh trùng và trao gửi đến tay bạn ly trà sữa tươi mát, đánh thức mọi giác quan.",
                IconCss = "bi bi-cup-straw",
                IsActive = true
            }
        );

        // Cấu hình Fluent API cho NewsArticle
        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(250);
            entity.Property(n => n.Category).IsRequired().HasMaxLength(100);
            entity.Property(n => n.Summary).IsRequired().HasMaxLength(1000);
            entity.Property(n => n.IsActive).HasDefaultValue(true);
        });

        // Seed Data: 4 bài viết Tin Tức & Khuyến Mãi
        modelBuilder.Entity<NewsArticle>().HasData(
            new NewsArticle
            {
                Id = 1,
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
                Id = 2,
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
                Id = 3,
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
                Id = 4,
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
    }
}
