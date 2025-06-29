using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Matrix.Models;

namespace Matrix.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult PrimeVueDemo()
    {
        // 假設這是從資料庫取出的產品列表
        List<Product> products = new List<Product>
        {
            new Product { Id = 1001, Name = "無線鍵盤", Category = "電子產品", Price = 1200m },
            new Product { Id = 1002, Name = "人體工學滑鼠", Category = "電子產品", Price = 1800m },
            new Product { Id = 1003, Name = "27吋螢幕", Category = "電子產品", Price = 7500m },
            new Product { Id = 1004, Name = "保溫杯", Category = "生活用品", Price = 650m }
        };

        // 將整個 List<Product> 作為 Model 傳遞給 View
        return View(products);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}