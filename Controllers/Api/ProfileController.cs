using Microsoft.AspNetCore.Mvc;
using Matrix.Services.Interfaces;
using Matrix.DTOs;

namespace Matrix.Controllers.Api
{
    /// <summary>
    /// 個人資料相關的 API Controller
    /// 提供個人資料的查詢和更新功能
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController(
        IUserService userService,
        IFileService fileService,
        ILogger<ProfileController> logger
    ) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IFileService _fileService = fileService;
        private readonly ILogger<ProfileController> _logger = logger;

        /// <summary>
        /// 更新個人資料
        /// 更新使用者的個人資料，包括顯示名稱、自我介紹、網站連結和密碼
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <param name="dto">個人資料 DTO</param>
        /// <returns>更新結果</returns>
        [HttpPut("{id}")]
        public async Task<ReturnType<object>> Update(Guid id, [FromBody] PersonDto dto)
        {
            try
            {
                _logger.LogInformation("開始更新個人資料，使用者 ID: {UserId}", id);

                var result = await _userService.UpdatePersonProfileAsync(id, dto);

                if (result.Success)
                {
                    _logger.LogInformation("個人資料更新成功，使用者 ID: {UserId}", id);
                }
                else
                {
                    _logger.LogWarning("個人資料更新失敗，使用者 ID: {UserId}，錯誤訊息: {Message}", id, result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新個人資料時發生例外，使用者 ID: {UserId}", id);
                return new ReturnType<object>
                {
                    Success = false,
                    Message = "更新過程中發生錯誤，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 取得用戶個人資料
        /// - GET /api/Profile           → 查詢目前登入者
        /// - GET /api/Profile/{username} → 以指定 username 查詢
        /// </summary>
        /// <param name="username">路由中的使用者名稱（可選）</param>
        /// <returns>指定用戶的個人資料</returns>
        [HttpGet("{username?}")]
        public async Task<ActionResult<PersonDto>> GetMyProfile([FromRoute] string? username = null)
        {
            try
            {
                Guid targetUserId;

                if (!string.IsNullOrWhiteSpace(username))
                {
                    // 以路由提供的 username 查詢
                    _logger.LogInformation("使用路由中的用戶名查詢: {Username}", username);
                    var user = await _userService.GetUserByUsernameAsync(username);
                    if (user == null)
                    {
                        _logger.LogWarning("找不到指定的用戶名: {Username}", username);
                        return NotFound("找不到該使用者");
                    }
                    targetUserId = user.UserId;
                }
                else
                {
                    // 不帶 username：使用當前認證使用者
                    var userIdFromContext = HttpContext.Items["UserId"] as Guid?;
                    if (!userIdFromContext.HasValue)
                    {
                        _logger.LogWarning("無法從認證中獲取用戶 ID，且未提供 username");
                        return Unauthorized("用戶未認證且未提供用戶名稱");
                    }
                    targetUserId = userIdFromContext.Value;
                    _logger.LogInformation("使用認證中的用戶 ID: {UserId}", targetUserId);
                }

                _logger.LogInformation("開始查詢用戶個人資料，使用者 ID: {UserId}", targetUserId);
                
                var personDto = await _userService.GetProfileByIdAsync(targetUserId);
                
                if (personDto == null)
                {
                    _logger.LogWarning("找不到用戶個人資料，使用者 ID: {UserId}", targetUserId);
                    return NotFound("找不到個人資料");
                }
                
                _logger.LogInformation("成功查詢用戶個人資料，使用者 ID: {UserId}", targetUserId);
                return Ok(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢用戶個人資料時發生例外");
                return StatusCode(500, "伺服器內部錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 更新當前用戶的個人資料
        /// 從認證中獲取用戶 ID，確保只能更新自己的資料
        /// </summary>
        /// <param name="dto">個人資料 DTO</param>
        /// <returns>更新結果</returns>
        [HttpPut("personal")]
        public async Task<ReturnType<object>> UpdateMyProfile([FromBody] PersonDto dto)
        {
            try
            {
                var userIdFromContext = HttpContext.Items["UserId"] as Guid?;
                if (!userIdFromContext.HasValue)
                {
                    _logger.LogWarning("無法從認證中獲取用戶 ID");
                    return new ReturnType<object> 
                    { 
                        Success = false, 
                        Message = "用戶未認證" 
                    };
                }
                var userId = userIdFromContext.Value;

                _logger.LogInformation("開始更新當前用戶個人資料，使用者 ID: {UserId}", userId);
                
                var result = await _userService.UpdatePersonProfileAsync(userId, dto);
                
                if (result.Success)
                {
                    _logger.LogInformation("當前用戶個人資料更新成功，使用者 ID: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("當前用戶個人資料更新失敗，使用者 ID: {UserId}，錯誤訊息: {Message}", userId, result.Message);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新當前用戶個人資料時發生例外");
                return new ReturnType<object> 
                { 
                    Success = false, 
                    Message = "更新過程中發生錯誤，請稍後再試" 
                };
            }
        }

        /// <summary>
        /// 上傳頭像或橫幅圖片
        /// 根據 type 參數決定更新 avatar 還是 banner
        /// </summary>
        /// <param name="file">上傳的圖片檔案</param>
        /// <param name="type">檔案類型：avatar 或 banner</param>
        /// <returns>上傳結果</returns>
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string type)
        {
            try
            {
                // 驗證輸入參數
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "請選擇要上傳的檔案" });

                if (string.IsNullOrEmpty(type) || (type != "avatar" && type != "banner"))
                    return BadRequest(new { success = false, message = "檔案類型必須是 avatar 或 banner" });

                // 檢查檔案類型
                if (!file.ContentType.StartsWith("image/"))
                    return BadRequest(new { success = false, message = "只允許上傳圖片檔案" });

                // 檢查檔案大小 (5MB)
                const long maxFileSize = 5 * 1024 * 1024;
                if (file.Length > maxFileSize)
                    return BadRequest(new { success = false, message = "檔案大小不可超過 5MB" });

                // 獲取當前用戶 ID
                var userIdFromContext = HttpContext.Items["UserId"] as Guid?;
                if (!userIdFromContext.HasValue)
                {
                    _logger.LogWarning("無法從認證中獲取用戶 ID");
                    return Unauthorized(new { success = false, message = "用戶未認證" });
                }
                var userId = userIdFromContext.Value;

                _logger.LogInformation("開始上傳 {Type} 圖片，使用者 ID: {UserId}", type, userId);

                // 使用 FileService 上傳檔案到 profile/imgs 目錄
                var relativePath = await _fileService.CreateFileAsync(file, "profile/imgs");
                
                if (string.IsNullOrEmpty(relativePath))
                {
                    _logger.LogError("檔案上傳失敗");
                    return StatusCode(500, new { success = false, message = "檔案上傳失敗" });
                }

                // 更新資料庫中對應的欄位
                var result = await _userService.UpdateProfileImageAsync(userId, type, relativePath);
                
                if (!result.Success)
                {
                    // 如果資料庫更新失敗，刪除已上傳的檔案
                    await _fileService.DeleteFileAsync(relativePath);
                    _logger.LogWarning("資料庫更新失敗，已刪除上傳的檔案: {Path}", relativePath);
                    return BadRequest(new { success = false, message = result.Message });
                }

                _logger.LogInformation("{Type} 圖片上傳成功，路徑: {Path}", type, relativePath);

                return Ok(new 
                { 
                    success = true, 
                    message = $"{(type == "avatar" ? "頭像" : "橫幅")}更新成功",
                    data = new { filePath = relativePath }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳圖片時發生例外，類型: {Type}", type);
                return StatusCode(500, new { success = false, message = "上傳過程中發生錯誤，請稍後再試" });
            }
        }

        /// <summary>
        /// 驗證密碼規則
        /// 提供前端即時驗證密碼是否符合server端規則
        /// </summary>
        /// <param name="password">要驗證的密碼</param>
        /// <returns>驗證結果</returns>
        [HttpPost("validate-password")]
        public IActionResult ValidatePassword([FromBody] ValidatePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return Ok(new { isValid = true, message = "" }); // 空密碼視為有效（表示不更改）
                }

                // 使用 UserService 的密碼驗證邏輯
                var validation = _userService.ValidatePassword(request.Password);
                
                return Ok(new 
                { 
                    isValid = validation.IsValid,
                    message = validation.IsValid ? "" : validation.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "密碼驗證時發生例外");
                return StatusCode(500, new { isValid = false, message = "驗證過程中發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取用戶文章中的圖片
        /// 返回指定用戶文章中的前N張圖片，按文章建立時間排序
        /// </summary>
        /// <param name="userId">可選的用戶 ID，如果不提供則使用認證中的 ID</param>
        /// <param name="count">圖片數量限制，預設為10</param>
        /// <returns>用戶圖片列表</returns>
        [HttpGet("images")]
        public async Task<ActionResult<List<UserImageDto>>> GetUserImages([FromQuery] Guid? userId = null, [FromQuery] int count = 10)
        {
            try
            {
                Guid targetUserId;
                
                if (userId.HasValue)
                {
                    // 使用外部傳入的 userId
                    targetUserId = userId.Value;
                    _logger.LogInformation("獲取用戶圖片，使用外部傳入的用戶 ID: {UserId}", targetUserId);
                }
                else
                {
                    // 從 HttpContext.Items 中獲取已認證的用戶 ID
                    var userIdFromContext = HttpContext.Items["UserId"] as Guid?;
                    if (!userIdFromContext.HasValue)
                    {
                        _logger.LogWarning("無法從認證中獲取用戶 ID，且未提供外部 userId");
                        return Unauthorized("用戶未認證且未提供用戶 ID");
                    }
                    targetUserId = userIdFromContext.Value;
                    _logger.LogInformation("獲取用戶圖片，使用認證中的用戶 ID: {UserId}", targetUserId);
                }

                // 驗證 count 參數
                if (count <= 0 || count > 50)
                {
                    count = 10; // 預設值
                }

                _logger.LogInformation("開始獲取用戶圖片，使用者 ID: {UserId}，數量限制: {Count}", targetUserId, count);
                
                var images = await _userService.GetUserImagesAsync(targetUserId, count);
                
                _logger.LogInformation("成功獲取用戶圖片，使用者 ID: {UserId}，返回 {ImageCount} 張圖片", targetUserId, images.Count);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶圖片時發生例外");
                return StatusCode(500, "伺服器內部錯誤，請稍後再試");
            }
        }
    }

    /// <summary>
    /// 密碼驗證請求模型
    /// </summary>
    public class ValidatePasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}
