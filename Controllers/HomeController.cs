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

    [AllowAnonymous]
    public IActionResult Index()
    {
        // 檢查用戶認證狀態
        var isAuthenticated = HttpContext.Items["IsAuthenticated"] as bool? ?? false;

        // 傳遞認證狀態給前端
        ViewBag.IsAuthenticated = isAuthenticated;

        // Hot List 改為前端透過 API 取得（/api/post/hot）
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
