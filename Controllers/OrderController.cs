using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Models;

namespace WebTraSua.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Order/Invoice/5
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng yêu cầu.");
            }

            // Kiểm tra phân quyền: Chỉ Admin hoặc chủ sở hữu đơn hàng mới được xem
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("UserId")?.Value;
            int.TryParse(userIdStr, out int currentUserId);

            if (!User.IsInRole("Admin") && order.UserId != currentUserId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(order);
        }

        // GET: /Order/MyOrders (Lịch sử đơn hàng của tôi)
        public async Task<IActionResult> MyOrders()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("UserId")?.Value;
            int.TryParse(userIdStr, out int currentUserId);

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.UserId == currentUserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
