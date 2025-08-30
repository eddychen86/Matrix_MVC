using Microsoft.AspNetCore.Mvc;
using Matrix.ViewModels;
using Matrix.Extensions;

namespace Matrix.ViewComponents
{
    public class CreatePostPopupViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var viewModel = new CreatePostPopupViewModel();
            
            // 使用與 MenuViewModel 相同的資料來源
            var auth = HttpContext.GetAuthInfo();
            
            if (auth.IsAuthenticated)
            {
                viewModel.UserName = auth.DisplayName;
                viewModel.UserImg = auth.AvatarPath ?? "";
            }
            
            return View(viewModel);
        }
    }
}