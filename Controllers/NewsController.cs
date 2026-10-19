using Microsoft.AspNetCore.Mvc;

namespace WebTraSua.Controllers;

public class NewsController : Controller
{
    // Đã gỡ bỏ trang Tin tức, tự động chuyển hướng về Trang chủ
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Product");
    }

    public IActionResult Details(int id)
    {
        return RedirectToAction("Index", "Product");
    }
}
