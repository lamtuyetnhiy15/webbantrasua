using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Helpers;
using WebTraSua.Models;

namespace WebTraSua.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // ĐĂNG NHẬP (LOGIN)
        // =====================================================================

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển hướng
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Product");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tìm user theo Email hoặc Username (không phân biệt hoa thường)
            var inputEmail = model.Email.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == inputEmail || (u.Username != null && u.Username.ToLower() == inputEmail));

            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            // Tạo danh sách Claims xác thực
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(12),
                AllowRefresh = true
            };

            // Đăng nhập qua Cookie Authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Đồng bộ Session để tương thích ngược
            HttpContext.Session.SetObject("User", user);
            HttpContext.Session.SetString("Username", user.Email);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);

            TempData["SuccessMessage"] = $"Chào mừng {user.FullName} đã quay trở lại TN Tea!";

            // Điều hướng
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Product");
        }

        // =====================================================================
        // ĐĂNG KÝ (REGISTER)
        // =====================================================================

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Product");
            }

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra Email đã tồn tại chưa
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Địa chỉ Email này đã được đăng ký. Vui lòng sử dụng email khác hoặc đăng nhập.");
                return View(model);
            }

            // Tạo người dùng mới với vai trò mặc định là "Customer"
            var newUser = new User
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLower(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = "Customer",
                CreatedAt = DateTime.Now,
                Username = model.Email.Trim().ToLower(),
                Password = model.Password
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký tài khoản TN Tea thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return RedirectToAction(nameof(Login));
        }

        // =====================================================================
        // ĐĂNG XUẤT (LOGOUT)
        // =====================================================================

        // GET / POST: /Account/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Bạn đã đăng xuất tài khoản thành công!";
            return RedirectToAction("Index", "Product");
        }

        // =====================================================================
        // TRANG TỪ CHỐI TRUY CẬP (ACCESS DENIED)
        // =====================================================================

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
