using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class NotifyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //GET:Notify
        [HttpGet]
        public IActionResult Notify()
        {
            var fakeNotify = new List<NotifyItemViewModel>
        {
            new NotifyItemViewModel {
                SenderName = "Eddy",
                SenderAvatarUrl = "~/static/img/Cute.png",
                Message = "Great artwork, love it!",
                TimeAgo = "2h"
            },
            new NotifyItemViewModel {
                SenderName = "Eason",
                SenderAvatarUrl = "~/static/img/Cute.png",
                Message = "Awesome job! Keep going!",
                TimeAgo = "5h"
            },
            new NotifyItemViewModel {
                SenderName = "Hung",
                SenderAvatarUrl = "~/static/img/Cute.png",
                Message = "Can I share this?",
                TimeAgo = "1d"
            }
        };
            return View("Notify",fakeNotify);
        }
    }
}
