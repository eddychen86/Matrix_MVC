using Microsoft.AspNetCore.Mvc;
using Matrix.Services;

namespace Matrix.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            var fakeProfile = new
            {
                banner = Url.Content("~/static/img/image-top.png"),
                avatarImg = Url.Content("~/static/img/foodpanda.jpg"),
                name = "Michael Jordan",
                isPrivate = false,
                overview = new
                {
                    intro = "xxxxxxxxxxxxxxxxxxxxxxxx",
                    email = "example@email.com",
                    extends = new[] {
                        "https://www.jordan1.com/",
                        "https://www.jordan2.com/",
                        "https://www.jordan3.com/"
                    },
                    images = new[] {
                        Url.Content("~/static/img/foodpanda.jpg"),
                        Url.Content("~/static/img/foodpanda.jpg"),
                        Url.Content("~/static/img/foodpanda.jpg")
                    }
                },
                articles = new[] {
                    new {
                        user = "Michael Jordan",
                        userImg = Url.Content("~/static/img/foodpanda.jpg"),
                        content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        images = new[] {
                            Url.Content("~/static/img/foodpanda.jpg"),
                            Url.Content("~/static/img/foodpanda.jpg")
                        },
                        createTime = "2023-10-01 12:00",
                        praises = 100,
                        collects = 50
                    },
                    new {
                        user = "Michael Jordan",
                        userImg = Url.Content("~/static/img/foodpanda.jpg"),
                        content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        images = new[] {
                            Url.Content("~/static/img/foodpanda.jpg"),
                            Url.Content("~/static/img/foodpanda.jpg")
                        },
                        createTime = "2023-10-01 12:00",
                        praises = 100,
                        collects = 50
                    }
                }
            };

            ViewBag.FakeProfile = fakeProfile;


            return View();
        }
    }
}
