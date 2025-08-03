using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Matrix.Data;
using Matrix.Models;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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

        // 取得文章資料（只讀，使用 AsNoTracking 優化）
        var articlesQuery = _context.Articles
            .AsNoTracking()
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
                    : null,
                file = a.Attachments
            })
            .ToListAsync();

        var hot_list = articles.Take(5);
        var default_list = articles;

        ViewBag.HotList = hot_list;
        ViewBag.DefaultList = default_list;

        //取得好友欄位資料
        var currentUserId = Guid.Parse("870c0b75-97a3-4e4f-8215-204d5747d28c");
        var friends = await _context.Friendships
            .Where(a =>
                (a.UserId == currentUserId || a.FriendId == currentUserId)
                && a.Status == FriendshipStatus.Accepted)
            .Select(a => a.UserId == currentUserId ? a.FriendId : a.UserId)
            .ToListAsync();

        var friendList = await _context.Users.Where(a => friends.Contains(a.UserId))
            .Include(a => a.Person)
            .Select(a => new Matrix.ViewModels.FriendListViewModel
            {
                UserId = a.UserId.ToString(),
                UserName = a.UserName,
                AvatarPath = (a.Person != null && a.Person.AvatarPath != null && a.Person.AvatarPath.Length > 0)
                    ? $"data:image/png;base64,{Convert.ToBase64String(a.Person.AvatarPath)}"
                    : "/static/img/default-avatar.png"
            })
            .ToListAsync();

        ViewBag.FriendList = friendList;

        //取得發文者資料
        var currentUser = await _context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        var currentUserVm = new CurrentUserViewModel
        {
            UserId = currentUser.UserId.ToString(),
            DisplayName = currentUser.UserName,
            Avatar = (currentUser.Person.AvatarPath != null && currentUser.Person.AvatarPath.Length > 0)
                ? $"data:image/png;base64,{Convert.ToBase64String(currentUser.Person.AvatarPath)}"
                : "/static/img/default-avatar.png"
        };
        ViewBag.CurrentUser = currentUserVm;


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
    public IActionResult TestConnection()
    {
        var followCount = _context.Follows.Count();
        return Content($"資料庫連線成功，目前共有 {followCount} 筆 Follow 資料");
    }
}