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

### 問題 6: Vue 3 響應式物件重置錯誤
**症狀**: 在 Vue 3 中使用 `reactive()` 創建的物件，執行重置函數時報錯或失去響應式

**原因**: 直接對 `reactive()` 物件進行賦值會丟失響應式代理參照

**錯誤寫法**:
```javascript
const formData = reactive({ userName: '', email: '' })

const resetFormData = () => {
  formData = {  // ❌ 直接賦值會失去響應式
    userName: '',
    email: ''
  }
}
```

**正確寫法**:
```javascript
const formData = reactive({ userName: '', email: '' })

// 方法1: 個別更新屬性
const resetFormData = () => {
  formData.userName = ''
  formData.email = ''
}

// 方法2: 使用 Object.assign
const resetFormData = () => {
  Object.assign(formData, {
    userName: '',
    email: ''
  })
}
```

**ref() 的差異**:
```javascript
// ref 可以重新賦值 .value
const formData = ref({ userName: '', email: '' })

const resetFormData = () => {
  formData.value = {  // ✅ ref 可以這樣重置
    userName: '',
    email: ''
  }
}
```

**解決方案**: 
- `reactive()` 物件：必須個別更新屬性或使用 `Object.assign()`
- `ref()` 物件：可以直接重新賦值 `.value` 或更新個別屬性
- 選擇 `reactive()` 適合複雜物件，`ref()` 適合簡單值或整個物件替換的場景

**相關檔案**: `/wwwroot/js/dashboard/pages/config/config.js:135-145`

### 問題 8: 實作管理員註冊服務與一般用戶註冊的差異化處理
**需求**: 建立獨立的管理員註冊流程，與一般用戶註冊不同：
- 不需要點擊驗證信 (status 預設為 1)
- DisplayName 預設為 UserName  
- 需要驗證身份 (Role 必須為 1 或 2)
- 呼叫地址為 '/Config/Register'

**解決方案**:

**步驟1: 修正 AdminRegistrationService.cs 中的角色驗證邏輯**
```csharp
// 錯誤寫法 - 邏輯錯誤，永遠為 true
if (model.Role != 1 || model.Role != 2)

// 正確寫法
if (model.Role <= 0 || (model.Role != 1 && model.Role != 2))
    validationErrors["Role"] = [_localizer["RoleRequired"]];
```

**步驟2: 修改 RegisterUserAsync 方法**
```csharp
// 建立 DTO (管理員註冊特殊設定)
var createUserDto = new CreateUserDto
{
    UserName = model.UserName,
    DisplayName = model.UserName, // DisplayName 自動設為 UserName
    Email = model.Email ?? string.Empty,
    Password = model.Password,
    PasswordConfirm = model.PasswordConfirm,
    Role = model.Role,  // 使用前端傳入的角色
    Status = 1  // 管理員註冊預設狀態為已驗證 (不需要點擊驗證信)
};
```

**步驟3: 建立 IAdminRegistrationService 介面**
```csharp
public interface IAdminRegistrationService
{
    Dictionary<string, string[]> ValidateRegistrationInput(AdminRegisterViewModel model);
    Task<(Guid? UserId, List<string> Errors)> RegisterUserAsync(AdminRegisterViewModel? model);
    Dictionary<string, string[]> MapServiceErrorsToFieldErrors(List<string> errors);
}
```

**步驟4: 修改 RegisterController.cs**
```csharp
// 建構函數加入依賴注入
public RegisterController(
    IUserRegistrationService registrationService,
    IAdminRegistrationService adminRegistrationService, // 新增
    ILogger<RegisterController> logger,
    IEmailService emailService,
    ICustomLocalizer localizer)

// AdminRegister 方法修改
[HttpPost("Config/Register")]
public async Task<IActionResult> AdminRegister([FromBody] AdminRegisterViewModel? model)
{
    // 使用管理員註冊服務
    var (userId, errors) = await _adminRegistrationService.RegisterUserAsync(model);
    
    // 成功後不發送驗證信，直接回傳成功訊息
    return ApiSuccess(new
    {
        userId = userId.Value.ToString(),
        userName = model.UserName,
        role = model.Role,
        status = "已驗證",
        redirectUrl = "/Config"
    }, _localizer["AdminRegisterSuccess"]);
}
```

**關鍵差異總結**:
- **一般用戶**: Status=0, 需要驗證信, DisplayName 可自訂, Role=0
- **管理員**: Status=1, 無需驗證信, DisplayName=UserName, Role=1或2

**相關檔案**:
- `/Services/AdminRegistrationService.cs`
- `/Services/Interfaces/IAdminRegistrationService.cs`  
- `/Controllers/Api/RegisterController.cs`
- `/Program.cs` (依賴注入配置)

### 問題 9: AdminRegistrationService 編譯錯誤修正
**症狀**: 
- AdminRegistrationService.cs 第10行介面引用錯誤
- AdminRegistrationService.cs 第102行 Status 屬性不存在錯誤  
- RegisterController.cs 第91行依賴注入錯誤

**原因**:
1. IAdminRegistrationService 介面未正確創建
2. CreateUserDto 沒有 Status 屬性，但程式碼嘗試設定
3. Program.cs 中未註冊 IAdminRegistrationService 服務

**解決方案**:

**步驟1: 修正 Status 屬性錯誤**
```csharp
// 錯誤寫法 - CreateUserDto 沒有 Status 屬性
var createUserDto = new CreateUserDto
{
    UserName = model.UserName,
    DisplayName = model.UserName,
    Email = model.Email ?? string.Empty,
    Password = model.Password,
    PasswordConfirm = model.PasswordConfirm,
    Role = model.Role,
    Status = 1  // ❌ 此屬性不存在
};

// 正確寫法 - 讓 UserService 根據 Role 自動設定狀態
var createUserDto = new CreateUserDto
{
    UserName = model.UserName,
    DisplayName = model.UserName, // DisplayName 自動設為 UserName
    Email = model.Email ?? string.Empty,
    Password = model.Password,
    PasswordConfirm = model.PasswordConfirm,
    Role = model.Role  // UserService 會自動根據 Role 設定 Status
};
```

**步驟2: UserService 自動狀態設定邏輯**
```csharp
// 在 UserService.CreateUserAsync 方法中 (第204行)
Status = dto.Role > 0 ? 1 : 0  // 管理員直接啟用，一般用戶需驗證
```

**步驟3: 添加依賴注入註冊**
```csharp
// Program.cs 中添加
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IAdminRegistrationService, AdminRegistrationService>(); // 新增此行
builder.Services.AddScoped<IAdminPermissionService, AdminPermissionService>();
```

**關鍵洞察**:
- UserService 已經內建了根據 Role 自動設定狀態的邏輯
- 管理員 (Role > 0) 自動設為已驗證狀態 (Status = 1)
- 一般用戶 (Role = 0) 需要電子郵件驗證 (Status = 0)

**相關檔案**:
- `/Services/AdminRegistrationService.cs:102`
- `/Services/UserService.cs:204`
- `/Program.cs:102`

### 問題 10: AdminRegistrationService 介面實作錯誤修復
**症狀**: 
- CS0101: 命名空間已包含 'IAdminRegistrationService' 定義
- CS0111: 類型已定義相同參數類型的成員 
- CS0535: AdminRegistrationService 未實作介面成員 'RegisterUserAsync(AdminRegisterViewModel?, int)'

**原因**:
1. 專案中存在重複的介面定義檔案 (`IAdminRegistrationService copy.cs`)
2. 介面要求的方法簽名與實作不符 (缺少 int role 參數)
3. 方法內部使用錯誤的參數來源 (使用 model.Role 而非 role 參數)
4. 缺少必要的 using 語句

**解決方案**:

**步驟1: 刪除重複的介面檔案**
```bash
rm "Services/Interfaces/IAdminRegistrationService copy.cs"
```

**步驟2: 修正介面方法簽名**
```csharp
// 原始介面要求
Task<(Guid? UserId, List<string> Errors)> RegisterUserAsync(AdminRegisterViewModel? model, int role = 1);

// 修正實作簽名
public async Task<(Guid? UserId, List<string> Errors)> RegisterUserAsync(AdminRegisterViewModel? model, int role = 1)
```

**步驟3: 修正參數使用**
```csharp
// 錯誤寫法 - 使用 model.Role
_logger.LogInformation("管理員註冊嘗試: {UserName}, 角色: {Role}", model?.UserName, model?.Role);
Role = model.Role  // ❌ 應該使用方法參數

// 正確寫法 - 使用 role 參數
_logger.LogInformation("管理員註冊嘗試: {UserName}, 角色: {Role}", model?.UserName, role);
Role = role  // ✅ 使用方法參數
```

**步驟4: 添加必要的 using 語句**
```csharp
using Matrix.Services.Interfaces;
using Matrix.ViewModels;
using Matrix.DTOs;
using Microsoft.Extensions.Logging; // 新增
```

**關鍵洞察**:
- C# 介面契約要求方法簽名完全匹配，包括參數順序和預設值
- 重複的介面定義檔案會導致命名衝突編譯錯誤
- 介面參數設計應該考慮方法的實際使用場景

**編譯驗證**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**相關檔案**:
- `/Services/AdminRegistrationService.cs:76,102,111,116`
- `/Services/Interfaces/IAdminRegistrationService.cs:29`

### 問題 7: Vue 元件屬性未定義錯誤 (Property was accessed during render but is not defined)
**症狀**: 在頁面中使用元件屬性時出現 "Property 'xxx' was accessed during render but is not defined on instance" 或 "Cannot read properties of undefined (reading 'xxx')" 錯誤

**原因**: 
1. 元件未正確導入到全域作用域
2. 元件返回的物件中缺少所需的屬性
3. 元件有條件載入但在其他頁面也被使用
4. 物件屬性初始化不完整，導致深層屬性訪問時出錯

**錯誤寫法**:
```javascript
// reply.js - 缺少所需屬性或初始化不安全
export const useReply = () => {
  const replyModal = ref({
    article: null  // ❌ 可能導致 article.xxx 時出錯
  })
  
  return {
    openReplyPopup  // ❌ 缺少其他屬性
  }
}

// main.js - 條件載入但全域使用
const Reply = LoadingPage(/^\/reply(?:\/|$)/i, useReply) // ❌ 只在特定路徑載入
```

**正確寫法**:
```javascript
// reply.js - 安全的屬性初始化
export const useReply = () => {
  const openReply = ref(false)
  const replyModal = ref({
    article: {           // ✅ 提供完整的預設結構
      id: null,
      title: '',
      content: '',
      author: null
    },
    isVisible: false,
    loading: false
  })
  
  const setReplyArticle = (article) => {
    if (article && typeof article === 'object') {  // ✅ 安全檢查
      replyModal.value.article = {
        id: article.id || article.articleId || null,
        title: article.title || '',
        content: article.content || '',
        author: article.author || article.authorName || null
      }
    }
  }
  
  return {
    openReplyPopup,
    openReply,
    replyModal,
    setReplyArticle
  }
}

// main.js - 全域載入元件
const Reply = (typeof useReply === 'function') ? useReply() : {} // ✅ 總是載入
```

**解決方案**: 
1. 檢查錯誤訊息中提到的屬性名稱和錯誤發生位置
2. 在對應元件中添加缺少的屬性，並提供安全的初始值
3. 對於物件類型的屬性，提供完整的預設結構而不是 null
4. 確保元件在需要的頁面都能正確載入
5. 在設置物件屬性時進行安全檢查

**偵錯技巧**:
- 使用瀏覽器開發者工具檢查 Vue DevTools
- 在元件的 return 物件中添加 console.log 調試
- 檢查錯誤堆疊中的具體行號，定位問題模板
- 使用 `?.` 可選鏈操作符在模板中安全訪問屬性

**相關檔案**: 
- `/wwwroot/js/components/reply.js`
- `/wwwroot/js/main.js:157`
- `/Views/Home/Index.cshtml:88-90`

---

## 待補充問題
*後續遇到的問題和解決方案會持續更新到此處*

---

**最後更新**: 2025-08-23  
**版本**: v1.7