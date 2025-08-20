using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    /// <summary>
    /// NFT 相關的 API Controller
    /// CRUD NFT 資訊
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NftController : ControllerBase
    {
        // 你指定的實體路徑與對外讀取路徑
        private const string NftPhysicalDir = @"C:\Users\lin05\OneDrive\Desktop\Matrix\wwwroot\public\NFTimgs";
        private const string NftRequestPrefix = "/public/NFTimgs";

        private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        /// <summary>
        /// 上傳 NFT 圖片
        /// POST /api/Nft/upload
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> Upload([FromForm] IFormFile? file, [FromForm] string? personId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "檔案不存在" });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
                return BadRequest(new { success = false, message = "只支援圖片檔（jpg、png、gif、webp、bmp）" });

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = "Content-Type 必須為 image/*" });



            Directory.CreateDirectory(NftPhysicalDir);

            var safeBase = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName), @"[^a-zA-Z0-9_\-]+", "_");
            if (safeBase.Length > 80) safeBase = safeBase[..80];
            var unique = $"{safeBase}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(NftPhysicalDir, unique);

            using (var fs = System.IO.File.Create(path))
            {
                await file.CopyToAsync(fs);
            }

            var fi = new FileInfo(path);
            var publicPath = $"{NftRequestPrefix}/{unique}".Replace("\\", "/");

            return Ok(new
            {
                success = true,
                data = new
                {
                    fileName = unique,
                    filePath = publicPath,
                    fileSize = fi.Length,
                    contentType = file.ContentType,
                    createTime = fi.CreationTimeUtc
                }
            });
        }

        /// <summary>
        /// 取得圖片清單
        /// GET /api/Nft/images
        /// </summary>
        [HttpGet("images")]
        public IActionResult List([FromQuery] int count = 10, [FromQuery] string? PersonId = null)
        {
            if (!Directory.Exists(NftPhysicalDir))
                return Ok(Array.Empty<object>());

            var files = Directory.EnumerateFiles(NftPhysicalDir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => AllowedExt.Contains(Path.GetExtension(p)))
                .Select(p => new FileInfo(p))
                .OrderByDescending(f => f.CreationTimeUtc)
                .Take(Math.Max(1, count))
                .Select(f => new
                {
                    fileId = Path.GetFileNameWithoutExtension(f.Name),
                    fileName = f.Name,
                    filePath = $"{NftRequestPrefix}/{f.Name}",
                    fileSize = f.Length,
                    createTime = f.CreationTimeUtc
                });

            return Ok(files);
        }
        //刪除圖片
        // Delete   /api/Nft/{fileName}
        [HttpDelete("{fileName}")]

        public IActionResult DeleteNFT([FromRoute]String fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) 
            {
                return Ok(new
                {
                    Ok = false,
                    message = "刪除失敗!"
                });
            }

            var fullPath = Path.Combine(NftPhysicalDir, fileName);
            if (!System.IO.File.Exists(fullPath)) 
            {
                return Ok(new
                {
                    Ok = false,
                    message = "刪除失敗!!"
                });
            }

            try
            {
                System.IO.File.Delete(fullPath);
                return Ok(new
                {
                    Ok = true,
                    message = "刪除成功!"
                });
            }
            catch (Exception ex) 
            {
                return Ok(new
                {
                    Ok = false,
                    message = $"刪除失敗:{ex.Message}"
                });
            }


        }
        
    }
}