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
        // 1. Tải danh mục thực tế từ Database để đồng bộ với trang quản lý
        var dbCategories = await _context.Categories.OrderBy(c => c.Id).ToListAsync();
        ViewBag.Categories = dbCategories;

        var allProducts = await _context.Products.ToListAsync();
        IEnumerable<Product> filtered = allProducts;

        // 1. Tìm kiếm theo từ khóa thông minh (hỗ trợ tiếng Việt có dấu, không dấu, hoa thường)
        bool hasSearch = !string.IsNullOrWhiteSpace(search);
        if (hasSearch)
        {
            var s = search!.Trim();
            filtered = filtered.Where(p => 
                WebTraSua.Helpers.SearchHelper.MatchesSearch(p.Name, s) ||
                WebTraSua.Helpers.SearchHelper.MatchesSearch(p.Description, s) ||
                WebTraSua.Helpers.SearchHelper.MatchesSearch(p.Badge, s) ||
                WebTraSua.Helpers.SearchHelper.MatchesSearch(p.Category, s)
            );
        }

        // 2. Lọc theo danh mục
        if (!string.IsNullOrWhiteSpace(category) && category != "all")
        {
            var catFiltered = FilterByCategory(filtered, category);
            // Nếu đang tìm kiếm mà danh mục hiện tại không có, nhưng các danh mục khác có món phù hợp,
            // tự động chuyển category về 'all' để khách hàng thấy món
            if (hasSearch && !catFiltered.Any() && filtered.Any())
            {
                category = "all";
            }
            else
            {
                filtered = catFiltered;
            }
        }

        var resultList = filtered.OrderBy(p => p.ProductId).ToList();

        const int pageSize = 12;
        int totalItems = resultList.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages < 1) totalPages = 1;

        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var products = resultList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.SelectedCategory = category ?? "all";
        ViewBag.SearchTerm = search ?? "";
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalAllItems = allProducts.Count;

        return View(products);
    }

    private static IEnumerable<Product> FilterByCategory(IEnumerable<Product> source, string category)
    {
        // Ưu tiên 1: Khớp chính xác tên mục lục
        var exact = source.Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
        if (exact.Any())
        {
            return exact;
        }

        // Ưu tiên 2: Hỗ trợ alias & tên mở rộng từ menu footer
        if (category == "trasua" || category == "damhuong" || category.Contains("Đậm Hương", StringComparison.OrdinalIgnoreCase))
        {
            return source.Where(p => (p.Category != null && p.Category.Contains("Đậm Hương", StringComparison.OrdinalIgnoreCase)) || (string.IsNullOrEmpty(p.Category) && p.Name.Contains("Trà Sữa", StringComparison.OrdinalIgnoreCase)));
        }
        if (category == "truyenthong" || category.Contains("Truyền Thống", StringComparison.OrdinalIgnoreCase))
        {
            return source.Where(p => (p.Category != null && p.Category.Contains("Truyền Thống", StringComparison.OrdinalIgnoreCase)) || (string.IsNullOrEmpty(p.Category) && (p.Name.Contains("Truyền Thống", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Đường Đen", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Assam", StringComparison.OrdinalIgnoreCase))));
        }
        if (category == "cheese" || category.Contains("cheese", StringComparison.OrdinalIgnoreCase) || category.Contains("kem trứng", StringComparison.OrdinalIgnoreCase) || category.Contains("matcha", StringComparison.OrdinalIgnoreCase))
        {
            return source.Where(p => (p.Category != null && (p.Category.Contains("Cheese", StringComparison.OrdinalIgnoreCase) || p.Category.Contains("Matcha", StringComparison.OrdinalIgnoreCase))) || (string.IsNullOrEmpty(p.Category) && (p.Name.Contains("Cheese", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Matcha", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Kem", StringComparison.OrdinalIgnoreCase))));
        }
        if (category == "tratraicay" || category.Contains("trái cây", StringComparison.OrdinalIgnoreCase))
        {
            return source.Where(p => (p.Category != null && p.Category.Contains("Trái Cây", StringComparison.OrdinalIgnoreCase)) || (string.IsNullOrEmpty(p.Category) && (p.Name.Contains("Trái Cây", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Đào", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Dâu", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Vải", StringComparison.OrdinalIgnoreCase))));
        }
        if (category == "topping" || category.Contains("topping", StringComparison.OrdinalIgnoreCase) || category.Contains("trân châu", StringComparison.OrdinalIgnoreCase))
        {
            return source.Where(p => (p.Category != null && p.Category.Contains("Topping", StringComparison.OrdinalIgnoreCase)) || p.Name.Contains("Trân Châu", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Pudding", StringComparison.OrdinalIgnoreCase));
        }
        return source.Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    // GET: /Product/SearchSuggestions?term=...
    [HttpGet]
    public async Task<IActionResult> SearchSuggestions(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(new List<object>());

        var s = term.Trim();
        var all = await _context.Products.ToListAsync();
        var matches = all
            .Where(p => 
                WebTraSua.Helpers.SearchHelper.MatchesSearch(p.Name, s) || 
                WebTraSua.Helpers.SearchHelper.MatchesSearch(p.Badge, s) || 
                WebTraSua.Helpers.SearchHelper.MatchesSearch(p.Category, s)
            )
            .Take(6)
            .Select(p => new
            {
                id = p.ProductId,
                name = p.Name,
                price = p.Price.ToString("N0") + " đ",
                imageUrl = p.ImageUrl,
                category = p.Category,
                badge = p.Badge
            })
            .ToList();

        return Json(matches);
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
