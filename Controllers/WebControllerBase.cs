using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    /// <summary>Web 控制器基底類別，提供 MVC 視圖相關的共用功能</summary>
    public abstract class WebControllerBase : Controller
    {
        /// <summary>返回 JSON 格式錯誤回應（用於 AJAX 請求）</summary>
        protected IActionResult JsonError(string message, object? errors = null)
        {
            return Json(new { success = false, message, errors });
        }

        /// <summary>返回 JSON 格式成功回應（用於 AJAX 請求）</summary>
        protected IActionResult JsonSuccess(object? data = null, string message = "操作成功")
        {
            return Json(new { success = true, message, data });
        }

        /// <summary>返回模型驗證錯誤的統一格式</summary>
        protected IActionResult ValidationErrorResponse()
        {
            var errors = ModelState
                .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return Json(new { success = false, errors });
        }
    }
}