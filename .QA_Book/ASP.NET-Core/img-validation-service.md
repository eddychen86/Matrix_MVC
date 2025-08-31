# 問題 27: 共用圖片驗證服務與數據清理

**症狀**: 需要在多個 Controller 中檢查圖片檔案是否真實存在，避免前端發送無效 HTTP 請求，並希望在 API 回應中直接清理無效的圖片路徑。

**原因**: 
- 資料庫中可能存有無效的圖片路徑（檔案已刪除、路徑錯誤等）
- 前端載入不存在的圖片會產生 404 請求，浪費帶寬和時間
- 每個 Controller 都需要類似的圖片驗證邏輯
- 希望在 API 層直接清理數據，讓前端接收乾淨的資料

**解決方案**: 
建立共用的 `IImageValidationService` 服務進行檔案驗證和數據清理

```csharp
// 1. 介面定義
public interface IImageValidationService
{
    Task<bool> IsImageExistsAsync(string imagePath);
    Task<Dictionary<string, bool>> ValidateImagesAsync(IEnumerable<string> imagePaths);
    Task<T> EnrichWithImageValidationAsync<T>(T item, params (string propertyName, string imagePath)[] imageProperties) where T : class;
    Task<IEnumerable<T>> EnrichWithImageValidationAsync<T>(IEnumerable<T> items, Func<T, IEnumerable<(string propertyName, string imagePath)>> imageSelector) where T : class;
}

// 2. 服務實作
public class ImageValidationService : IImageValidationService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ImageValidationService> _logger;
    private readonly ConcurrentDictionary<string, bool> _validationCache = new();

    public async Task<bool> IsImageExistsAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath)) return false;

        // 快取檢查
        if (_validationCache.TryGetValue(imagePath, out bool cachedResult))
            return cachedResult;

        try {
            var result = await Task.Run(() => {
                var fullPath = GetFullPath(imagePath);
                return File.Exists(fullPath);
            });

            _validationCache.TryAdd(imagePath, result);
            return result;
        } catch (Exception ex) {
            _logger.LogError(ex, "檢查圖片時發生錯誤: {ImagePath}", imagePath);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> ValidateImagesAsync(IEnumerable<string> imagePaths)
    {
        var tasks = imagePaths
            .Where(path => !string.IsNullOrEmpty(path))
            .Select(async path => new { Path = path, Exists = await IsImageExistsAsync(path) });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(r => r.Path, r => r.Exists);
    }

    private string GetFullPath(string imagePath)
    {
        var cleanPath = imagePath.TrimStart('/', '\\');
        return Path.Combine(_webHostEnvironment.WebRootPath, cleanPath);
    }
}
```

**Controller 中的使用 (數據清理方式)**:
```csharp
[HttpGet("hot")]
public async Task<IActionResult> GetHot([FromQuery] int count = 10)
{
    var items = list.Select(a => new { /* 原始數據對應 */ }).ToList();

    // 收集所有需要檢查的圖片路徑
    var imagePaths = new List<string>();
    foreach (var item in items)
    {
        if (item.image?.FilePath != null) imagePaths.Add(item.image.FilePath);
        if (!string.IsNullOrEmpty(item.authorAvatar)) imagePaths.Add(item.authorAvatar);
    }

    // 批量檢查圖片存在性
    var validationResults = await _imageValidationService.ValidateImagesAsync(imagePaths);

    // 根據檢查結果清理數據
    var cleanedItems = items.Select(item => new
    {
        // ... 其他欄位
        // 頭像：無效時設為空字串
        authorAvatar = !string.IsNullOrEmpty(item.authorAvatar) && 
                      validationResults.GetValueOrDefault(item.authorAvatar, false) 
                      ? item.authorAvatar : "",
        // 主圖片：無效時設為 null
        image = item.image != null && 
               !string.IsNullOrEmpty(item.image.FilePath) && 
               validationResults.GetValueOrDefault(item.image.FilePath, false)
               ? item.image : null
    }).ToList();

    return Ok(new { items = cleanedItems });
}
```

**服務註冊**:
```csharp
// Program.cs
builder.Services.AddScoped<IImageValidationService, ImageValidationService>();
```

**前端使用 (簡化版)**:
```html
<!-- 主圖片 -->
<img v-if="item.image && item.image.filePath" :src="item.image.filePath" />
<div v-else class="no-img">無圖片</div>

<!-- 頭像 -->
<img v-if="item.authorAvatar && item.authorAvatar !== ''" :src="item.authorAvatar" />
<div v-else class="default-avatar">{{ authorName[0] }}</div>
```

**技術優勢**:
- **性能提升**: 批量檢查使用 `Task.WhenAll` 平行處理
- **快取機制**: 使用 `ConcurrentDictionary` 避免重複檢查
- **數據一致性**: API 層直接清理無效路徑，前端接收乾淨數據
- **可重用性**: 任何 Controller 都可以使用此服務
- **錯誤處理**: 完整的例外處理和日誌記錄
- **泛型設計**: 支援各種物件類型的圖片驗證

**效能比較**:
- **檔案檢查**: < 1ms per file
- **HTTP 404 請求**: 10-50ms per request
- **減少網絡流量**: 避免無效圖片的 404 響應
- **改善用戶體驗**: 無圖片載入失敗的閃爍效果

**相關檔案**: 
- `Services/Interfaces/IImageValidationService.cs` - 服務介面
- `Services/ImageValidationService.cs` - 服務實作
- `Controllers/Api/PostController.cs:66-111` - 使用範例
- `Program.cs:103` - 服務註冊
- `Views/Home/Index.cshtml:23-24,37-40` - 前端使用