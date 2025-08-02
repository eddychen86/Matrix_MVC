using Microsoft.AspNetCore.Http;

namespace Matrix.Services
{
    public interface ICustomLocalizer
    {
        string this[string key] { get; }
    }

    public class CustomLocalizer : ICustomLocalizer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomLocalizer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string this[string key]
        {
            get
            {
                // 獲取當前語言，預設為繁體中文
                var currentCulture = GetCurrentCulture();
                
                // 使用共用的翻譯服務
                return TranslationService.GetTranslation(currentCulture, key);
            }
        }

        private string GetCurrentCulture()
        {
            // 從 Cookie 中獲取語言設定
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Cookies.ContainsKey(".AspNetCore.Culture") == true)
            {
                var cultureCookie = httpContext.Request.Cookies[".AspNetCore.Culture"];
                if (!string.IsNullOrEmpty(cultureCookie) && cultureCookie.Contains("c="))
                {
                    var culture = cultureCookie.Split('|')[0].Replace("c=", "");
                    return culture;
                }
            }
            
            // 預設返回繁體中文
            return "zh-TW";
        }
    }
}