using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Matrix.Models;
using Matrix.Data;
using Matrix.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // 取得所有文章（lazy loading 會自動載入 Author）
        var articles = await _context.Articles
            .Include(a => a.Attachments)
            .OrderByDescending(a => a.CreateTime)
            .Select(a => new
            {
                Article = a,
                image = a.Attachments != null
                    ? a.Attachments.FirstOrDefault(att => att.Type.ToLower() == "image")
                    : null
            })
            .ToListAsync();
        var hot_list = articles.Take(5);
        var default_list = articles;

        ViewBag.HotList = hot_list;
        ViewBag.DefaultList = default_list;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}