using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Helpers;
using WebTraSua.Models;

namespace WebTraSua.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _context;
    private const string CartSessionKey = "Cart";

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    // Helper: Lấy giỏ hàng từ Session
    private List<CartItem> GetCart()
    {
        return HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
    }

    // Helper: Lưu giỏ hàng vào Session
    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetObject(CartSessionKey, cart);
    }

    // GET: /Cart/
    public IActionResult Index()
    {
        var cart = GetCart();
        ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
        return View(cart);
    }

    // POST & GET: /Cart/AddToCart/5
    public async Task<IActionResult> AddToCart(int id, int quantity = 1, string? returnUrl = null)
    {
        if (quantity <= 0) quantity = 1;

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Không tìm thấy món." });
            }
            return NotFound();
        }

        var cart = GetCart();
        var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

        if (cartItem != null)
        {
            cartItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });
        }

        SaveCart(cart);
        var successMessage = $"Đã thêm \"{product.Name}\" vào giỏ hàng!";
        TempData["SuccessMessage"] = successMessage;

        var totalCount = cart.Sum(c => c.Quantity);

        if (IsAjaxRequest())
        {
            return Json(new
            {
                success = true,
                message = successMessage,
                cartCount = totalCount,
                productName = product.Name
            });
        }

        // Ưu tiên 1: Chuyển hướng theo returnUrl nếu hợp lệ
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        // Ưu tiên 2: Chuyển hướng quay lại trang trước đó (Referer header)
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            if (string.Equals(refererUri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(referer);
            }
        }

        return RedirectToAction("Index");
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || Request.Headers["Accept"].ToString().Contains("application/json");
    }

    // POST & GET: /Cart/RemoveFromCart/5
    public IActionResult RemoveFromCart(int id)
    {
        var cart = GetCart();
        var itemToRemove = cart.FirstOrDefault(c => c.ProductId == id);

        if (itemToRemove != null)
        {
            cart.Remove(itemToRemove);
            SaveCart(cart);
            TempData["SuccessMessage"] = $"Đã xóa sản phẩm khỏi giỏ hàng.";
        }

        return RedirectToAction("Index");
    }

    // POST & GET: /Cart/UpdateQuantity/5?delta=1 hoặc delta=-1
    public IActionResult UpdateQuantity(int id, int delta)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == id);

        if (item != null)
        {
            item.Quantity += delta;
            if (item.Quantity <= 0)
            {
                cart.Remove(item);
                TempData["SuccessMessage"] = $"Đã xóa món \"{item.ProductName}\" khỏi giỏ hàng.";
            }
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/Checkout
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        // 1. Kiểm tra giỏ hàng
        var cart = GetCart();
        if (cart == null || cart.Count == 0)
        {
            TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
            return RedirectToAction("Index", "Product");
        }

        // 2. Lấy UserId từ Claims của User đang đăng nhập
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("UserId")?.Value;

        int userId = 0;
        if (!int.TryParse(userIdStr, out userId))
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                userId = user.Id;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản người dùng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Index" });
            }
        }

        // 3. Tạo đơn hàng mới (Order)
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalAmount = cart.Sum(c => c.TotalPrice)
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Lưu để sinh OrderId

        // 4. Lưu danh sách chi tiết đơn hàng (OrderDetails)
        foreach (var item in cart)
        {
            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            };
            _context.OrderDetails.Add(orderDetail);
        }

        await _context.SaveChangesAsync();

        // 5. Làm trống giỏ hàng
        HttpContext.Session.Remove(CartSessionKey);

        TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã hóa đơn của bạn là #{order.OrderId}.";

        // 6. Chuyển hướng tới màn hình Hóa đơn
        return RedirectToAction("Invoice", "Order", new { id = order.OrderId });
    }
}
