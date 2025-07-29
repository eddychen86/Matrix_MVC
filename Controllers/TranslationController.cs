using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Matrix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly IStringLocalizerFactory _localizerFactory;

        public TranslationController(IStringLocalizerFactory localizerFactory)
        {
            _localizerFactory = localizerFactory;
        }

        [HttpGet("{culture}")]
        public IActionResult GetTranslations(string culture)
        {
            try
            {
                // 設定當前文化
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);

                // 建立翻譯字典
                var translations = new Dictionary<string, string>();

                // 載入各個模組的翻譯
                LoadTranslations(translations, "Views.Common.Menu", culture);
                LoadTranslations(translations, "Views.Auth.Auth", culture);
                LoadTranslations(translations, "Views.Home.Home", culture);
                LoadTranslations(translations, "Views.Common.Common", culture);

                return Ok(translations);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading translations: {ex.Message}");
            }
        }

        private void LoadTranslations(Dictionary<string, string> translations, string resourceName, string culture)
  {
            var localizer = _localizerFactory.Create(resourceName, "Matrix");

            // 根據不同的資源檔載入不同的 keys
            string[] keys;

            switch (resourceName)
            {
                case "Views.Common.Menu":
                    keys = new[] {
                        "Matrix",
                        "Login",
                        "Search",
                        "Notify",
                        "Follows",
                        "Collects",
                        "Language",
                        "HideBar",
                        "LogOut"
                    };
                    break;
                case "Views.Common.Common":
                    keys = new[] {
                        "OK",
                        "Cancel",
                        "Yes",
                        "No",
                        "Save",
                        "Edit",
                        "Delete",
                        "Create",
                        "Submit",
                        "Loading",
                        "Error",
                        "Success",
                        "Comment",
                        "Praise",
                        "Follow",
                        "Collect",
                        "Share"
                    };
                    break;
                case "Views.Auth.Auth":
                    keys = new[] {
                        "Login",
                        "Register",
                        "Email",
                        "UserName",
                        "Password",
                        "ConfirmPassword"
                    };
                    break;
                case "Views.Home.Home":
                    keys = new[] {
                        "Title",
                        "Welcome"
                    }; // 根據你的 Home.resx 內容調整
                    break;
                default:
                    keys = new string[0];
                    break;
            }

            foreach (var key in keys)
            {
                var translation = localizer[key];
                if (!translation.ResourceNotFound)
                {
                    translations[key] = translation.Value;
                }
            }
        }
    }
}