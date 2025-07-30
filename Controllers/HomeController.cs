using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        // 檢查用戶認證狀態
        var isAuthenticated = HttpContext.Items["IsAuthenticated"] as bool? ?? false;
        var isGuest = HttpContext.Items["IsGuest"] as bool? ?? false;

        // 根據認證狀態決定文章數量限制
        int articleLimit = isAuthenticated ? int.MaxValue : 10; // 訪客只能看10篇

        // 取得文章資料
        var articlesQuery = _context.Articles
            .Include(a => a.Attachments)
            .Include(a => a.Author)
            .Where(a => a.IsPublic == 0) // 只顯示公開文章
            .OrderByDescending(a => a.CreateTime);

        // 根據認證狀態限制文章數量
        var articles = await articlesQuery
            .Take(articleLimit)
            .Select(a => new
            {
                Article = a,
                Author = a.Author,
                image = a.Attachments != null
                    ? a.Attachments.FirstOrDefault(att => att.Type.ToLower() == "image")
                    : null
            })
            .ToListAsync();

        var hot_list = articles.Take(5);
        var default_list = articles;

        ViewBag.HotList = hot_list;
        ViewBag.DefaultList = default_list;

        // 傳遞認證狀態給前端
        ViewBag.IsAuthenticated = isAuthenticated;
        ViewBag.IsGuest = isGuest;
        ViewBag.ArticleLimit = articleLimit;
        ViewBag.TotalPublicArticles = await _context.Articles.CountAsync(a => a.IsPublic == 0);

        _logger.LogInformation(
            "Index loaded - Authenticated: {IsAuthenticated}, Articles shown: {ArticleCount}/{TotalCount}",
            isAuthenticated,
            articles.Count(),
            (int)ViewBag.TotalPublicArticles
        );

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}