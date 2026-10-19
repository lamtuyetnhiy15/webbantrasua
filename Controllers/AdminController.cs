using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Models;

namespace WebTraSua.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin or /Admin/Index
        // Dashboard thống kê tổng quan
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            var orderAmounts = await _context.Orders.Select(o => o.TotalAmount).ToListAsync();
            ViewBag.TotalRevenue = orderAmounts.Sum();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            // 5 Đơn hàng mới nhất
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }

        // GET: /Admin/Products
        // Quản lý Sản phẩm / Trà sữa
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .OrderBy(p => p.ProductId)
                .ToListAsync();
            return View(products);
        }

        private async Task LoadCategoriesToViewBagAsync()
        {
            var cats = await _context.Categories.OrderBy(c => c.Id).ToListAsync();
            if (!cats.Any())
            {
                var defaultCats = new[] { "Trà Sữa Đậm Hương", "Trà Sữa Truyền Thống", "Cheese & Matcha", "Trà Trái Cây Tươi Mát", "Topping & Món Khác" };
                foreach (var c in defaultCats)
                {
                    _context.Categories.Add(new Category { Name = c });
                }
                await _context.SaveChangesAsync();
                cats = await _context.Categories.OrderBy(c => c.Id).ToListAsync();
            }
            ViewBag.Categories = cats;
        }

        // GET: /Admin/CreateProduct
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            await LoadCategoriesToViewBagAsync();
            return View(new Product { Price = 35000, Badge = "Tn Signature" });
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? imageUpload)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên món trà sữa.");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0 đ.");
            }

            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh nếu có
                if (imageUpload != null && imageUpload.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(imageUpload.FileName);
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);
                    var filePath = Path.Combine(imagesFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageUpload.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/images/" + fileName;
                }
                else if (string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    product.ImageUrl = "https://images.unsplash.com/photo-1558857563-b37df35d1f86?w=600&auto=format&fit=crop&q=80";
                }

                product.Category = string.IsNullOrWhiteSpace(product.Category) ? "Trà Sữa Đậm Hương" : product.Category.Trim();
                product.Badge = string.IsNullOrWhiteSpace(product.Badge) ? null : product.Badge.Trim();

                // Tự động thêm vào bảng Categories nếu chưa có
                if (!await _context.Categories.AnyAsync(c => c.Name.ToLower() == product.Category.ToLower()))
                {
                    _context.Categories.Add(new Category { Name = product.Category });
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm thành công món \"{product.Name}\" vào Menu TN Tea!";
                return RedirectToAction(nameof(Products));
            }

            await LoadCategoriesToViewBagAsync();
            return View(product);
        }

        // GET: /Admin/EditProduct/5
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy món trà sữa yêu cầu.";
                return RedirectToAction(nameof(Products));
            }
            await LoadCategoriesToViewBagAsync();
            return View(product);
        }

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? imageUpload)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError("Name", "Tên món trà sữa không được để trống.");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0 đ.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Products.FindAsync(id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.Name = product.Name.Trim();
                    existing.Price = product.Price;
                    existing.Category = string.IsNullOrWhiteSpace(product.Category) ? "Trà Sữa Đậm Hương" : product.Category.Trim();
                    existing.Badge = string.IsNullOrWhiteSpace(product.Badge) ? null : product.Badge.Trim();
                    existing.Description = product.Description;

                    // Nếu có tải ảnh mới lên
                    if (imageUpload != null && imageUpload.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(imageUpload.FileName);
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);
                        var filePath = Path.Combine(imagesFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageUpload.CopyToAsync(stream);
                        }
                        existing.ImageUrl = "/images/" + fileName;
                    }

                    // Tự động thêm vào bảng Categories nếu chưa có
                    if (!await _context.Categories.AnyAsync(c => c.Name.ToLower() == existing.Category.ToLower()))
                    {
                        _context.Categories.Add(new Category { Name = existing.Category });
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật món \"{existing.Name}\" thành công!";
                    return RedirectToAction(nameof(Products));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            await LoadCategoriesToViewBagAsync();
            return View(product);
        }

        // AJAX: Lấy danh sách mục lục (Categories)
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var list = await _context.Categories.OrderBy(c => c.Id).Select(c => new { c.Id, c.Name }).ToListAsync();
            return Json(list);
        }

        // AJAX: Thêm mục lục mới
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name))
            {
                return Json(new { success = false, message = "Tên mục lục không được để trống." });
            }

            var trimmedName = dto.Name.Trim();
            var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower());
            if (exists)
            {
                return Json(new { success = false, message = "Mục lục này đã tồn tại." });
            }

            var cat = new Category { Name = trimmedName };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();
            return Json(new { success = true, category = new { cat.Id, cat.Name } });
        }

        // AJAX: Sửa tên mục lục
        [HttpPost]
        public async Task<IActionResult> EditCategory([FromBody] EditCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.NewName))
            {
                return Json(new { success = false, message = "Tên mục lục mới không được để trống." });
            }

            var cat = await _context.Categories.FindAsync(dto.Id);
            if (cat == null)
            {
                return Json(new { success = false, message = "Không tìm thấy mục lục." });
            }

            var oldName = cat.Name;
            var newName = dto.NewName.Trim();

            // Cập nhật tên mục lục
            cat.Name = newName;

            // Cập nhật tên mục lục cho tất cả các sản phẩm đang thuộc mục lục cũ
            var productsToUpdate = await _context.Products.Where(p => p.Category == oldName).ToListAsync();
            foreach (var p in productsToUpdate)
            {
                p.Category = newName;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, category = new { cat.Id, cat.Name } });
        }

        // AJAX: Xóa mục lục
        [HttpPost]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryDto dto)
        {
            var cat = await _context.Categories.FindAsync(dto.Id);
            if (cat == null)
            {
                return Json(new { success = false, message = "Không tìm thấy mục lục." });
            }

            var name = cat.Name;

            // Chuyển các món đang có danh mục bị xóa về danh mục còn lại
            var fallbackCat = await _context.Categories.Where(c => c.Id != dto.Id).OrderBy(c => c.Id).FirstOrDefaultAsync();
            var fallbackName = fallbackCat?.Name ?? "Trà Sữa Đậm Hương";

            var productsToUpdate = await _context.Products.Where(p => p.Category == name).ToListAsync();
            foreach (var p in productsToUpdate)
            {
                p.Category = fallbackName;
            }

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = dto.Id });
        }

        // POST: /Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy món trà sữa cần xóa.";
                return RedirectToAction(nameof(Products));
            }

            // Kiểm tra xem sản phẩm đã có đơn hàng nào chưa
            var hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = $"Không thể xóa món \"{product.Name}\" vì đã có trong lịch sử hóa đơn của khách! Bạn có thể chỉnh sửa giá hoặc tên món thay vì xóa.";
                return RedirectToAction(nameof(Products));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa thành công món \"{product.Name}\" khỏi Menu!";
            return RedirectToAction(nameof(Products));
        }

        // GET: /Admin/Orders
        // Quản lý Đơn hàng / Hóa đơn
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // GET: /Admin/Users
        // Quản lý Người dùng & Phân quyền
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View(users);
        }
    }

    public class CategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class EditCategoryDto
    {
        public int Id { get; set; }
        public string NewName { get; set; } = string.Empty;
    }

    public class DeleteCategoryDto
    {
        public int Id { get; set; }
    }
}
