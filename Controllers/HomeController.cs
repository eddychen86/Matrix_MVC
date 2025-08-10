using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Matrix.Data;
using Matrix.Models;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;
using Matrix.Repository.Interfaces;

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

        // Hot List 改為前端透過 API 取得（/api/post/hot）
        return View();
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
