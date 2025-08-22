# Matrix 專案 - 疑難雜症合集

## JavaScript 相關問題

### 問題 1: PermissionService 不是函數錯誤
**症狀**: 在 `config.js:274` 行出現 "PermissionService is not a function" 錯誤

**原因**: `PermissionService` 是定義為物件，包含多個方法，不是可直接呼叫的函數

**錯誤寫法**:
```javascript
await PermissionService()  // ❌ 試圖呼叫物件當作函數
```

**正確寫法**:
```javascript
// 移除錯誤的呼叫，或呼叫特定方法：
await PermissionService.getUserPermissions()  // ✅ 呼叫物件的方法
```

**解決方案**: 
- 如果不需要初始化，直接移除 `await PermissionService()` 這行
- 如果需要初始化，呼叫具體的方法如 `getUserPermissions()`

---

## ASP\.NET Core 相關問題

### 問題 2: 無法取得 Claims 資訊進行調試
**症狀**: 在 Controller 中無法正確查看 Claims 內容

**解決方案**: 在 `GetCurrentUserInfoAsync()` 方法中添加詳細的 Claims 調試資訊

```csharp
// 詳細記錄所有 Claims 資訊
_logger.LogInformation("\n=== User Claims Debug Info ===");
foreach (var claim in User.Claims)
{
    _logger.LogInformation("Type: {Type}, Value: {Value}", claim.Type, claim.Value);
}
_logger.LogInformation("=== End Claims Debug ===\n");
```

**多重 UserId 檢查**:
```csharp
var userIdClaim = User.FindFirst("UserId")?.Value 
    ?? User.FindFirst("sub")?.Value 
    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
```

### 問題 3: 篩選特定類型的 Claims
**需求**: 只想查看 `Type == "Role"` 的 Claims 值

**解決方案**:
```csharp
// 方法1: 使用 LINQ 篩選
foreach (var claim in User.Claims.Where(c => c.Type == "Role"))
{
    _logger.LogInformation("Type: {Type}, Value: {Value}", claim.Type, claim.Value);
}

// 方法2: 直接查找第一個符合的
var roleClaim = User.FindFirst("Role")?.Value;
_logger.LogInformation("Role Claim Value: {RoleClaim}", roleClaim);
```

**其他常用 Claims 操作**:
```csharp
// 檢查是否包含特定 Claim
bool hasRoleClaim = User.HasClaim("Role", "Admin");

// 取得多種 Claim 類型
var importantClaims = User.Claims.Where(c => 
    c.Type == "Role" || c.Type == "UserId" || c.Type == "Email");
```

---

## 程式設計原則與洞察

### JavaScript 物件 vs 函數
- **物件**: 包含多個方法的容器，如 `PermissionService = { method1(), method2() }`
- **函數**: 可直接執行的程式碼，如 `function doSomething() {}`
- **使用**: 物件需要透過 `.` 語法呼叫其方法

### Claims 調試最佳實務
- 使用結構化日誌記錄 Claims 資訊
- 考慮多種可能的 Claim 類型名稱
- 使用 LINQ 進行彈性的 Claims 查詢
- 在開發環境啟用詳細的 Claims 日誌

### 服務物件模式
- 將相關功能分組到單一物件中
- 按需呼叫特定方法，避免不必要的初始化
- 保持方法的單一職責原則

---

## Git 相關問題

### 問題 4: 搜尋所有分支中包含特定關鍵字的內容
**症狀**: 需要在所有 git 分支中搜尋特定關鍵字（檔案名稱、資料夾、程式碼片段）

**解決方案**:

**方法1: 搜尋 commit 訊息**
```bash
# 搜尋所有分支的 commit 訊息中包含關鍵字
git log --all --grep="關鍵字" -i --oneline
```

**方法2: 搜尋檔案內容變更**
```bash
# 搜尋所有分支中檔案內容包含關鍵字的 commit
git log --all --source -S"關鍵字" -i --oneline
```

**方法3: 搜尋當前所有分支的檔案內容**
```bash
# 在所有分支中搜尋檔案內容
for branch in $(git branch -a | sed 's/^[* ] //' | sed 's/remotes\///' | sort -u); do 
  echo "=== Branch: $branch ==="; 
  git grep -i "關鍵字" $branch 2>/dev/null || echo "No matches found"; 
  echo; 
done
```

**方法4: 搜尋特定分支的檔案內容**
```bash
# 搜尋特定分支
git grep -i "關鍵字" 分支名稱

# 搜尋當前分支
git grep -i "關鍵字"
```

**常用參數**:
- `-i`: 不區分大小寫
- `--all`: 包含所有分支
- `--source`: 顯示來源分支
- `--oneline`: 簡潔顯示
- `-S"文字"`: 搜尋檔案內容變更

**相關檔案**: 適用於任何 git 專案

### 問題 5: 從舊 commit 選擇性合併特定檔案到當前分支
**症狀**: 需要從歷史 commit 中恢復或合併特定檔案，而不是整個 commit

**解決方案**:

**方法1: 使用 git checkout 恢復特定檔案**
```bash
# 從特定 commit 恢復單個檔案
git checkout <commit-hash> -- <檔案路徑>

# 從特定 commit 恢復多個檔案
git checkout <commit-hash> -- <檔案1> <檔案2> <檔案3>

# 從特定 commit 恢復整個資料夾
git checkout <commit-hash> -- <資料夾路徑>/
```

**方法2: 使用 git show 和重定向**
```bash
# 查看特定 commit 的檔案內容並保存
git show <commit-hash>:<檔案路徑> > <檔案路徑>

# 例如：git show 058cf02:Services/SignalRService.cs > Services/SignalRService.cs
```

**方法3: 使用 git cherry-pick 選擇性應用變更**
```bash
# 只套用 commit 中特定檔案的變更（需要互動模式）
git cherry-pick -n <commit-hash>  # -n 不自動 commit
git reset HEAD <不要的檔案>        # 取消不需要的檔案
git commit                        # 手動 commit 想要的變更
```

**方法4: 使用 git restore（Git 2.23+）**
```bash
# 從特定 commit 恢復檔案
git restore --source=<commit-hash> <檔案路徑>

# 從特定分支恢復檔案
git restore --source=<分支名> <檔案路徑>
```

**實用技巧**:
- 恢復檔案後需要 `git add` 和 `git commit` 來保存變更
- 使用 `git status` 檢查哪些檔案被修改
- 使用 `git diff` 查看變更內容
- 可以先在新分支測試，確認無誤再合併

**相關檔案**: 適用於任何 git 專案

---

## 待補充問題
*後續遇到的問題和解決方案會持續更新到此處*

---

**最後更新**: 2025-08-22  
**版本**: v1.2