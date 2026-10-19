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
    public async Task<IActionResult> AddToCart(int id, int quantity = 1)
    {
        if (quantity <= 0) quantity = 1;

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
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
        TempData["SuccessMessage"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng!";

        return RedirectToAction("Index");
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
