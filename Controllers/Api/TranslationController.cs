using Microsoft.AspNetCore.Mvc;
using Matrix.Services;

namespace Matrix.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        [HttpGet("{culture}")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Language")]
        public IActionResult GetTranslations(string culture)
        {
            // 檢查是否為支援的語言
            if (!TranslationService.IsSupportedCulture(culture))
            {
                return BadRequest($"Unsupported culture: {culture}");
            }

            // 設置高性能快取標頭
            Response.Headers["Cache-Control"] = "public, max-age=86400, immutable, stale-while-revalidate=3600";
            Response.Headers["ETag"] = $"\"{culture}-v1.0\"";
            Response.Headers["Vary"] = "Accept-Encoding";
            
            // 檢查客戶端快取
            if (Request.Headers["If-None-Match"] == $"\"{culture}-v1.0\"")
            {
                return StatusCode(304); // Not Modified
            }

            // 使用共用的翻譯服務
            var translations = TranslationService.GetTranslations(culture);
            
            // 設置額外的性能標頭
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["X-Frame-Options"] = "DENY";
            
            return Ok(translations);
        }
    }
}