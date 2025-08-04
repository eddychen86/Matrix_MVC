using Microsoft.AspNetCore.Mvc;
using Matrix.Services;

namespace Matrix.Controllers.Api
{
    /// <summary>API 控制器基底類別，提供統一的 API 回應格式和錯誤處理</summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>返回標準的成功回應</summary>
        protected IActionResult ApiSuccess(object? data = null, string message = "操作成功")
        {
            return Ok(new { success = true, message, data });
        }

        /// <summary>返回標準的錯誤回應</summary>
        protected IActionResult ApiError(string message, object? errors = null)
        {
            return BadRequest(new { success = false, message, errors });
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
            
            return BadRequest(new { success = false, errors });
        }
    }
}