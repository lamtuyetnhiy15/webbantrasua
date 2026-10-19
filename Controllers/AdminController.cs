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

        // GET: /Admin/CreateProduct
        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View(new Product { Price = 35000 });
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

                if (string.IsNullOrWhiteSpace(product.Category))
                {
                    product.Category = "Trà Sữa Đậm Hương";
                }
                else
                {
                    product.Category = product.Category.Trim();
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm thành công món \"{product.Name}\" vào Menu TN Tea!";
                return RedirectToAction(nameof(Products));
            }

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
                    else if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                    {
                        existing.ImageUrl = product.ImageUrl.Trim();
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

            return View(product);
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
}
