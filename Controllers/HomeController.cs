using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Models;

namespace WebTraSua.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Product");
    }

    // GET: /Home/Story (Chuyện TN Tea)
    public async Task<IActionResult> Story()
    {
        var milestones = await _context.BrandStoryMilestones
            .Where(m => m.IsActive)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync();

        var craftingSteps = await _context.CraftingSteps
            .Where(s => s.IsActive)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();

        var viewModel = new BrandStoryViewModel
        {
            Milestones = milestones,
            CraftingSteps = craftingSteps
        };

        return View(viewModel);
    }

    // GET: /Home/News (Tin tức - đã bỏ, chuyển hướng về Trang chủ)
    public IActionResult News()
    {
        return RedirectToAction("Index", "Product");
    }

    // GET: /Home/Contact (Liên hệ)
    public IActionResult Contact()
    {
        return View();
    }

    // POST: /Home/Contact
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string fullName, string email, string phone, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ Họ tên và Số điện thoại liên hệ.";
            return View();
        }

        TempData["SuccessMessage"] = $"Cảm ơn {fullName}! TN Tea đã tiếp nhận thông tin và sẽ liên hệ lại với bạn trong vòng 15 phút.";
        return RedirectToAction(nameof(Contact));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
