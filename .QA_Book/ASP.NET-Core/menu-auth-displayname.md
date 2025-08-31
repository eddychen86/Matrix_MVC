# 問題 31: CommonController 指向不存在的屬性導致 CS1061（auth.displayName）

**症狀**:
```
/Controllers/CommonController.cs(30,36): error CS1061: 'AuthInfo' does not contain a definition for 'displayName' and no accessible extension method 'displayName' accepting a first argument of type 'AuthInfo' could be found (are you missing a using directive or an assembly reference?)
```

**原因**:
- `AuthInfo` 類型的屬性為 `DisplayName`（帕斯卡命名），但程式碼誤用了 `auth.displayName`（駝峰命名、小寫開頭），C# 為大小寫敏感，導致編譯器找不到成員。
- 另外，`MenuViewModel` 並沒有 `DisplayName` 欄位，將該值指定到 `MenuViewModel` 亦無意義，應以 `UserName` 作為側邊選單顯示名稱。

**錯誤寫法**:
```csharp
return new MenuViewModel
{
    IsAuthenticated = isLogin,
    DisplayName = auth.displayName, // ❌ AuthInfo 沒有 displayName，且 MenuViewModel 也沒有 DisplayName 屬性
    UserName = displayUserName,
    // ...
};
```

**正確寫法**:
- 直接移除不存在的 `DisplayName` 指派，並將顯示用名稱賦值給 `UserName`（原本已有）。
```csharp
return new MenuViewModel
{
    IsAuthenticated = isLogin,
    UserName = displayUserName, // 來自 AuthInfo.DisplayName / UserName 的安全化顯示字串
    UserRole = auth.Role,
    UserId = auth.UserId,
    IsGuest = !isLogin,
    UserImg = safeAvatarPath,
    // ...
};
```

或若確有需要同時保留完整顯示名稱，可在 `MenuViewModel` 中新增 `DisplayName` 屬性，並使用正確的帕斯卡命名：
```csharp
public class MenuViewModel
{
    public string? DisplayName { get; set; }
    // ...
}

// 賦值：
DisplayName = auth.DisplayName,
```

**解決方案**:
- 修正成員命名錯誤（`displayName` → `DisplayName`），或直接移除該賦值以符合 `MenuViewModel` 的實際欄位。
- 本專案採用移除該無效欄位的方式，並使用 `UserName = displayUserName` 進行顯示。

**相關檔案**: `Controllers/CommonController.cs`, `Extensions/CookieExtension.cs`, `ViewModels/MenuViewModel.cs`
