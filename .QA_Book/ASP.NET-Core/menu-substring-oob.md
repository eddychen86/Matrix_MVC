# 問題 32: 使用者名稱 Substring 超出長度導致 ArgumentOutOfRangeException

**症狀**:
```
System.ArgumentOutOfRangeException: Index and length must refer to a location within the string. (Parameter 'length')
    at System.String.Substring(Int32 startIndex, Int32 length)
    at Matrix.Controllers.CommonController.BuildMenuModel(HttpContext context)
```

**原因**:
- 在 `CommonController.BuildMenuModel` 中，為了縮短顯示名稱採用：
  ```csharp
  var displayUserName = safeUserName.Length > 5 ? safeUserName.Substring(0, 8) + "..." : safeUserName;
  ```
- 判斷條件是 `> 5`，但卻固定 `Substring(0, 8)`。當字串長度為 6 或 7 時，會發生超界而拋出 `ArgumentOutOfRangeException`。

**正確寫法**:
- 使用一致的上限判斷，或採用 `Math.Min`/範圍運算避免越界。
```csharp
const int maxUserNameLen = 8;
var displayUserName = safeUserName.Length > maxUserNameLen
    ? safeUserName.Substring(0, maxUserNameLen) + "..."
    : safeUserName;
// 或： safeUserName[..Math.Min(safeUserName.Length, maxUserNameLen)]
```

**解決方案**:
- 將條件與截斷長度對齊，避免任何長度落在 6~7 的邊界情況造成例外。

**相關檔案**: `Controllers/CommonController.cs`
