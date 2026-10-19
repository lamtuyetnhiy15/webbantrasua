using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTraSua.Models;

namespace WebTraSua.Controllers;

public class StoryController : Controller
{
    private readonly AppDbContext _context;

    public StoryController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Story hoặc /Story/Index
    public async Task<IActionResult> Index()
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

        return View("~/Views/Home/Story.cshtml", viewModel);
    }
}
