using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Models;

namespace WebTraSua.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Product/ hoặc /
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products.ToListAsync();
        return View(products);
    }

    // GET: /Product/Menu
    public async Task<IActionResult> Menu(string? category, string? search, int page = 1)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) || 
                                     p.Description.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "all")
        {
            if (category == "trasua")
            {
                query = query.Where(p => p.Name.Contains("Trà Sữa"));
            }
            else if (category == "truyenthong" || category == "tratraicay")
            {
                query = query.Where(p => p.Name.Contains("Truyền Thống") || p.Name.Contains("Đường Đen") || p.Name.Contains("Assam") || p.Name.Contains("Trà Đào"));
            }
            else if (category == "cheese")
            {
                query = query.Where(p => p.Name.Contains("Cheese") || p.Name.Contains("Matcha") || p.Name.Contains("Kem"));
            }
        }

        const int pageSize = 12;
        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages < 1) totalPages = 1;

        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var products = await query
            .OrderBy(p => p.ProductId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SelectedCategory = category ?? "all";
        ViewBag.SearchTerm = search ?? "";
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalAllItems = await _context.Products.CountAsync();

        return View(products);
    }

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
}
