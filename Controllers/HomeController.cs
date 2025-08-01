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
        // 取得所有文章（lazy loading 會自動載入 Author）
        var articles = await _context.Articles
            .Include(a => a.Attachments)
            .Include(a => a.Author)
            .OrderByDescending(a => a.CreateTime)
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
                AvatarPath = a.Person != null ? a.Person.AvatarPath : null
            })
            .ToListAsync();

        //假好友資料
        var friendListdmeo = new List<Matrix.ViewModels.FriendListViewModel>
        {
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "1",
                UserName = "DoGG",
                AvatarPath = "https://images.unsplash.com/photo-1518717758536-85ae29035b6d?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "2",
                UserName = "短腿橘貓",
                AvatarPath = "https://images.unsplash.com/photo-1518715308788-3005759c61d4?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "3",
                UserName = "呆萌虎斑",
                AvatarPath = "https://images.unsplash.com/photo-1515378791036-0648a3ef77b2?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "4",
                UserName = "WhiteDoGG",
                AvatarPath = "https://images.unsplash.com/photo-1465101046530-73398c7f28ca?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "5",
                UserName = "藍眼貓咪",
                AvatarPath = "https://images.unsplash.com/photo-1508672019048-805c876b67e2?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "6",
                UserName = "貓咪老大",
                AvatarPath = "https://images.unsplash.com/photo-1518715308788-3005759c61d4?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "7",
                UserName = "黑白喵喵",
                AvatarPath = "https://images.unsplash.com/photo-1500534314209-a25ddb2bd429?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "8",
                UserName = "雙下巴貓",
                AvatarPath = "https://images.unsplash.com/photo-1518715058639-2d3db6be0b18?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "9",
                UserName = "微笑虎斑",
                AvatarPath = "https://images.unsplash.com/photo-1518717758536-85ae29035b6d?auto=format&fit=facearea&w=80&h=80"
            },
            new Matrix.ViewModels.FriendListViewModel
            {
                UserId = "10",
                UserName = "瞇眼橘貓",
                AvatarPath = "https://images.unsplash.com/photo-1518715308788-3005759c61d4?auto=format&fit=facearea&w=80&h=80"
            }

        };

        ViewBag.FriendList = friendList;
        ViewBag.FriendListdmeo = friendListdmeo;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}