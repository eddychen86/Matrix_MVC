using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Matrix.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NftController : ControllerBase
    {
        private const string NftPhysicalDir = @"C:\Users\lin05\OneDrive\Desktop\Matrix\wwwroot\public\NFTimgs";
        private const string NftRequestPrefix = "/public/NFTimgs";
        private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        /// <summary>
        /// 上傳 NFT 圖片 (純資料夾方案)
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> Upload([FromForm] IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "檔案不存在" });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
                return BadRequest(new { success = false, message = "只支援圖片檔" });

            // 1. 從後端獲取當前登入者 ID，用來決定要存到哪個資料夾
            var userIdFromContext = HttpContext.Items["UserId"] as Guid?;
            if (!userIdFromContext.HasValue)
            {
                return Unauthorized(new { success = false, message = "用戶未認證，無法上傳" });
            }
            var currentUserIdStr = userIdFromContext.Value.ToString();

            // 2. 建立使用者專屬的資料夾路徑
            var userDirectory = Path.Combine(NftPhysicalDir, currentUserIdStr);
            Directory.CreateDirectory(userDirectory); // 如果資料夾不存在，就建立它

            // 3. 產生唯一檔名並儲存檔案到該使用者的資料夾
            var safeBase = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName), @"[^a-zA-Z0-9_\-]+", "_");
            var uniqueFileName = $"{safeBase}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
            var physicalPath = Path.Combine(userDirectory, uniqueFileName);

            await using (var fs = System.IO.File.Create(physicalPath))
            {
                await file.CopyToAsync(fs);
            }

            // 4. 產生可供前端讀取的公開路徑 (URL)
            var publicPath = $"{NftRequestPrefix}/{currentUserIdStr}/{uniqueFileName}".Replace("\\", "/");

            return Ok(new { success = true, data = new { filePath = publicPath, fileName = uniqueFileName } });
        }

        /// <summary>
        /// 取得圖片清單 (純資料夾方案)
        /// </summary>
        [HttpGet("images")]
        public IActionResult ListImages([FromQuery] int count = 30, [FromQuery] string? userId = null)
        {
            // 必須提供 personId 才能知道要讀取哪個資料夾
            if (string.IsNullOrWhiteSpace(userId))
            {
                // 改為回傳空陣列，避免前端報錯
                return Ok(new List<object>());
            }

            var userDirectory = Path.Combine(NftPhysicalDir, userId);

            // 如果該使用者還沒有上傳過任何圖片 (資料夾不存在)，就回傳空陣列
            if (!Directory.Exists(userDirectory))
            {
                return Ok(new List<object>());
            }

            var files = Directory.EnumerateFiles(userDirectory, "*.*")
                .Where(p => AllowedExt.Contains(Path.GetExtension(p)))
                .Select(p => new FileInfo(p))
                .OrderByDescending(f => f.CreationTimeUtc)
                .Take(count)
                .Select(f => new
                {
                    // 這裡回傳的資料格式要和前端需要的 userNFTs 物件一致
                    fileId = Path.GetFileNameWithoutExtension(f.Name),
                    fileName = f.Name,
                    filePath = $"{NftRequestPrefix}/{userId}/{f.Name}".Replace("\\", "/"), // URL 也要包含使用者 ID
                    // 根據前端 modal 內的 isOwner 判斷，可能需要 user id
                    // 為了簡化，我們先假設前端 modal 暫時不需要 user id
                });

            return Ok(files);
        }

        /// <summary>
        /// 刪除圖片 (純資料夾方案)
        /// </summary>
        [HttpDelete("{fileName}")]
        public IActionResult DeleteNFT(string fileName)
        {
            // 1. 取得當前登入者 ID，用來確認他只能刪除自己資料夾內的檔案
            var userIdFromContext = HttpContext.Items["UserId"] as Guid?;
            if (!userIdFromContext.HasValue)
            {
                return Unauthorized(new { success = false, message = "用戶未認證" });
            }
            var currentUserIdStr = userIdFromContext.Value.ToString();

            // 2. 組合出檔案在該使用者資料夾內的完整路徑
            var physicalPath = Path.Combine(NftPhysicalDir, currentUserIdStr, fileName);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(new { success = false, message = "找不到檔案或無權限刪除" });
            }

            try
            {
                System.IO.File.Delete(physicalPath);
                return Ok(new { success = true, message = "刪除成功" });
            }
            catch (Exception ex)
            {
                // 可以記錄 Log
                return StatusCode(500, new { success = false, message = $"刪除失敗: {ex.Message}" });
            }
        }
    }
}