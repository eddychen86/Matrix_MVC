using Microsoft.AspNetCore.Mvc;
using Matrix.Services;

namespace Matrix.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {

        [HttpGet("{culture}")]
        public IActionResult GetTranslations(string culture)
        {
            // 檢查是否為支援的語言
            if (!TranslationService.IsSupportedCulture(culture))
            {
                return BadRequest($"Unsupported culture: {culture}");
            }

            // 使用共用的翻譯服務
            var translations = TranslationService.GetTranslations(culture);
            return Ok(translations);
        }

    }
}