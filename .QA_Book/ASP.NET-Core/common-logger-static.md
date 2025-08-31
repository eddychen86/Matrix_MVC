# 問題 30: CommonController 使用 Primary Constructor 注入 ILogger 導致 CS9105/CS9113

**症狀**: 編譯輸出如下錯誤與警告

```
/Controllers/CommonController.cs(7,40): warning CS9113: Parameter '_logger' is unread.
/Controllers/CommonController.cs(14,13): error CS9105: Cannot use primary constructor parameter 'ILogger<ActivityLogController> _logger' in this context.
```

**原因**:
- 在 C# 12 的 Primary Constructor 中注入的參數僅能在「具體允許的實例成員」中使用，不能在 `static` 成員裡使用；在 `static` 方法中使用會觸發 CS9105。
- 此外，型別還錯用了 `ILogger<ActivityLogController>`，應該使用 `ILogger<CommonController>`。
- 由於參數無法在 static 內容中使用，編譯器也會回報該參數未被讀取 (CS9113)。

**錯誤寫法**:
```csharp
public class CommonController(
    ILogger<ActivityLogController> _logger
) : Controller
{
    public static MenuViewModel BuildMenuModel(HttpContext context)
    {
        _logger.LogInformation("..."); // ← static 方法中使用，觸發 CS9105
        // ...
    }
}
```

**正確寫法**:
- 保留 `static` 輔助方法的同時，透過 `HttpContext.RequestServices` 從 DI 解析 logger；不使用 Primary Constructor。
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class CommonController : Controller
{
    public static MenuViewModel BuildMenuModel(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<CommonController>>();
        logger.LogInformation("\\n\\nAuth:\n{auth}", auth);
        // ...
    }
}
```

**解決方案**:
- 移除 `CommonController` 的 Primary Constructor 注入。
- 在 `BuildMenuModel` 內，使用 `context.RequestServices.GetRequiredService<ILogger<CommonController>>()` 取得 logger。
- 同時修正泛型型別為 `ILogger<CommonController>`。

**相關檔案**: `Controllers/CommonController.cs`

