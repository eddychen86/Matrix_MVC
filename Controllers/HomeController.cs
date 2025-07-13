using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Matrix.Models;
using Matrix.Data;

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

    public IActionResult Index()
    {
        var fakeArticles = new List<Article>();
        var fakeUsers = new List<Person>();

        for (int i = 1; i <= 10; i++)
        {
            // https://img.daisyui.com/images/stock/photo-1606107557195-0e29a4b5b4aa.webp
            fakeArticles.Add(new Article
            {
                ArticleId = Guid.NewGuid().ToString(),
                AuthorId = $"Author{i}", // 這裡不用管資料庫有沒有這個人
                Content = $"Cat ipsum dolor sit amet, quia eiusmod velitesse. Explicabo architecto incidunt excepteur laudantium. Voluptas. Velitesse ullamco or excepteur, and error so veritatis. Laboris ex so enim, non yet suscipit illum cupidatat. Eaque perspiciatis veniam but dicta or ut so ipsa. Qui quam. Voluptate.",
                IsPublic = 0,
                Status = 0,
                CreateTime = DateTime.Now.AddMinutes(-i * 10),
                PraiseCount = 999,
                CollectCount = 999,
                Author = null, // 或給一個 Person 物件
                Replies = new List<Reply>(),
                PraiseCollects = new List<PraiseCollect>(),
                Attachments = new List<ArticleAttachment>()
            });
        }

        for (int i = 0; i <= 5; i++)
        {
            var fakeIdentityUser = new IdentityUser
            {
                Id = $"AspNetUserId{i}", // 你想要的 Id
                UserName = $"user{i}@test.com",
                Email = $"user{i}@test.com"
                // 其他必要欄位可視情況補上
            };

            fakeUsers.Add(new Person
            {
                User = fakeIdentityUser,
                PersonId = Guid.NewGuid().ToString(),
                Status = 0,
                DisplayName = $"",
                AvatarPath = "",
                IsPrivate = 0,
                WalletAddress = "",
                ModifyTime = DateTime.Now.AddMinutes(0),
            });
        }

        return View(fakeArticles);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}