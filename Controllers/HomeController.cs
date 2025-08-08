using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Matrix.Data;
using Matrix.Models;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;

namespace Matrix.Controllers;

public class HomeController : WebControllerBase
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

        var currentUserId = Guid.Parse("870c0b75-97a3-4e4f-8215-204d5747d28c");
        var currentUser = await _context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        var currentUserVm = new CurrentUserViewModel
        {
            UserId = currentUser?.UserId.ToString() ?? string.Empty,
            DisplayName = currentUser?.UserName ?? string.Empty,
            Avatar = currentUser?.Person?.AvatarPath ?? ""
        };
        ViewBag.CurrentUser = currentUserVm;

        // 傳遞認證狀態給前端
        ViewBag.IsAuthenticated = isAuthenticated;
        ViewBag.IsGuest = isGuest;
        ViewBag.ArticleLimit = articleLimit;
        ViewBag.TotalPublicArticles = await _context.Articles.CountAsync(a => a.IsPublic == 0);

        _logger.LogInformation(
            "Index loaded - Authenticated: {IsAuthenticated}, ArticleLimit: {ArticleLimit}, TotalCount: {TotalCount}",
            isAuthenticated,
            articleLimit,
            (int)ViewBag.TotalPublicArticles
        );

        return View(); // 不再傳遞 articles，改用 API
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult TestConnection()
    {
        var followCount = _context.Follows.Count();
        return Content($"資料庫連線成功，目前共有 {followCount} 筆 Follow 資料");
    }
}