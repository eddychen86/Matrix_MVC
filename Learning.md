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

### 問題 11: Vue v-model 使用可選鏈結運算子導致 Invalid left-hand side in assignment
**症狀**: 在瀏覽器 console 出現「Uncaught SyntaxError: Invalid left-hand side in assignment」，發生於 `Areas/Dashboard/Views/Config/index.cshtml` 第27–43行（`v-model` 綁定處）。

**原因**: `v-model` 會被 Vue 編譯為賦值表達式，例如 `model[key] = $event`。若在 `v-model` 的左側使用可選鏈結運算子（如 `obj?.[key]`），編譯後會變成 `obj?.[key] = ...`，這在 JavaScript 中是無效的賦值左值，因此觸發語法錯誤。

**錯誤寫法**:
```html
<input v-model="adminFilterVal?.[tool.title]">
<input type="checkbox" v-model="adminFilterVal?.[tool.title]">
```

**正確寫法**:
```html
<input v-model="adminFilterVal[tool.title]">
<input type="checkbox" v-model="adminFilterVal[tool.title]">
```

**解決方案**: 
- 將 `v-model` 左側的可選鏈結 `?.` 移除，確保是可指派的目標。
- 確保 `adminFilterVal` 於初始化時為物件（例如 `ref({})` 或含預設鍵值的物件），避免取值時為 `null/undefined`。
- 若擔心鍵不存在，可在初始化時先建立對應鍵（例如 `Keyword/SuperAdmin/Status`）。
- 檢查其他相似寫法（如被註解的 `logFilterVal?.[tool.title]`）並一併改為不含 `?.`。

**相關檔案**: 
- `/Areas/Dashboard/Views/Config/index.cshtml:27-43, 240-256`
- `/wwwroot/js/dashboard/pages/config/config.js`（`adminFilterVal` 初始化位置）

### 問題 12: 配置管理頁面的完整功能實作與 v-for 優化
**需求**: 完善配置管理頁面的所有功能，包含管理員列表、新增、編輯、刪除、篩選、分頁等，並使用 v-for 結構減少重複程式碼。

**解決方案**:

**步驟1: JavaScript 變數初始化完善**
```javascript
// 新增分頁相關變數
const adminList_totalPages = ref(1)
const adminList_totalCount = ref(0)

// 改善 getAdminsAsync 支援篩選
const getAdminsAsync = async (useFilter = false) => {
  const requestBody = {
    page: adminList_curPage.value,  // 修正: 使用 page 而非 pages
    pageSize: adminList_pageSize.value
  }
  
  if (useFilter) {
    // 篩選邏輯實作
    const filterConditions = {}
    if (adminFilterVal.value.Config_AdminList_Keyword) {
      filterConditions.keyword = adminFilterVal.value.Config_AdminList_Keyword
    }
    // ... 其他篩選條件
    requestBody.filters = filterConditions
  }
  
  // 更新分頁資訊
  adminList_totalPages.value = result.totalPages || 1
  adminList_totalCount.value = result.totalCount || 0
}
```

**步驟2: 管理員 CRUD 操作實作**
```javascript
const editAdmin = async (id) => {
  const admin = adminList.data.find(a => a.id === id)
  if (admin) {
    admin.isEditing = true
    admin.originalData = { /* 備份原始資料 */ }
  }
}

const saveAdmin = async (id) => {
  // PUT /api/Config/Update 實作
}

const delAdmin = async (id) => {
  // DELETE /api/Config/Delete/{id} 實作
  if (!confirm('確定要刪除此管理員嗎？')) return
}
```

**步驟3: HTML 模板 v-for 結構優化**
```html
<ul class="flex flex-col gap-4">
  <li v-for="tool in adminFilter" :key="tool.id">
    <!-- 文字搜尋 -->
    <template v-if="tool.type === 'text'">
      <div class="flex flex-col gap-2">
        <h5 class="subTitle">{{ tool.title }}</h5>
        <input type="search" 
               v-model="adminFilterVal[tool.title]" 
               :placeholder="tool.placeholder || '搜尋...'" 
               @input="applyFilter" />
      </div>
    </template>

    <!-- 開關選項 -->
    <template v-else-if="tool.type === 'switch'">
      <div class="flex items-center justify-between">
        <h5 class="subTitle">{{ tool.title }}</h5>
        <div class="tabs tabs-box">
          <input type="radio" 
                 v-for="opt in switchOpts" 
                 :key="opt.id" 
                 v-model="adminFilterVal[tool.title]" 
                 :value="opt.id" 
                 @change="applyFilter" />
        </div>
      </div>
    </template>
  </li>
</ul>
```

**步驟4: 動態配置物件擴展**
```javascript
const adminFilter = ref([
  { 
    id: 0, 
    title: 'Config_AdminList_Keyword', 
    type: 'text',
    placeholder: '搜尋使用者名稱或信箱...'
  },
  { id: 1, title: 'Config_AdminList_SuperAdmin', type: 'switch' },
  { id: 2, title: 'Config_AdminList_Status', type: 'switch' },
])

const logFilter = ref([
  { id: 0, title: 'Config_LoginList_Keyword', type: 'text' },
  { id: 1, title: 'Config_LoginList_Role', type: 'switch' },
  { id: 2, title: 'Config_LoginList_Browser', type: 'Number' },
  { 
    id: 6, 
    title: 'Config_LoginList_ActionType', 
    type: 'select',
    options: [
      { value: 'login', label: '登入' },
      { value: 'logout', label: '登出' }
    ]
  },
  { id: 3, title: 'Config_LoginList_LoginTime', type: 'DateTime' },
])
```

**步驟5: 分頁功能修正**
```javascript
// 修正分頁變數引用
const toggleLastPage = async () => {
  if (adminList_curPage.value !== adminList_totalPages.value) {  // 使用正確變數
    adminList_curPage.value = adminList_totalPages.value
    await getAdminsAsync()
  }
}
```

**關鍵改進**:
1. **統一的 v-for 架構**: 支援 text、switch、Number、DateTime、select 等多種控件類型
2. **動態模板渲染**: 透過 `<template>` 標籤根據配置物件自動渲染對應的 UI 元件
3. **響應式篩選**: 輸入變更時自動觸發篩選功能
4. **完整的 CRUD 操作**: 支援編輯模式、資料備份還原、API 呼叫
5. **正確的分頁處理**: 使用正確的變數名稱和邏輯

**相關檔案**:
- `/wwwroot/js/dashboard/pages/config/config.js:21-24, 80-143, 295-398`
- `/Areas/Dashboard/Views/Config/index.cshtml:25-71, 222-272, 282-348`

### 問題 13: 配置管理頁面多語言 placeholder 整合
**症狀**: JavaScript 中使用硬編碼的 placeholder 文字，未與既有的翻譯系統整合。

**原因**: JavaScript 配置物件中直接設定中文 placeholder，而專案已建立完整的 TranslationService.cs 多語言架構。

**錯誤寫法**:
```javascript
const adminFilter = ref([
  { 
    id: 0, 
    title: 'Config_AdminList_Keyword', 
    type: 'text',
    placeholder: '搜尋使用者名稱或信箱...'  // ❌ 硬編碼中文
  }
])
```

**正確寫法**:
```javascript
const adminFilter = ref([
  { 
    id: 0, 
    title: 'Config_AdminList_Keyword', 
    type: 'text',
    placeholderKey: 'Config_AdminList_KeywordPlaceholder'  // ✅ 使用翻譯 key
  }
])
```

**HTML 模板修正**:
```html
<!-- 錯誤寫法 -->
<input :placeholder="tool.placeholder || '搜尋...'" />

<!-- 正確寫法 -->
<input :data-i18n-placeholder="tool.placeholderKey || ''"
       :placeholder="@Localizer[\"tool.placeholderKey\"]" />
```

**TranslationService.cs 擴充**:
```csharp
// 新增共用按鈕翻譯
["Apply"] = "套用",
["Clear"] = "清除",

// 英文版本
["Apply"] = "Apply",
["Clear"] = "Clear",
```

**解決方案**:
1. JavaScript 配置物件使用 `placeholderKey` 指向翻譯 key
2. HTML 模板透過 `@Localizer` 系統渲染正確語言
3. 確保所有 UI 文字都支援多語言切換
4. 遵循既有的 `Config_` 前綴命名規範

**關鍵優點**:
- **一致性**: 所有文字都通過翻譯系統管理
- **可維護性**: 單一位置管理所有翻譯內容  
- **國際化支援**: 自動支援新增語言版本
- **標準化**: 遵循專案既有的翻譯架構

**相關檔案**:
- `/wwwroot/js/dashboard/pages/config/config.js:53-91`
- `/Areas/Dashboard/Views/Config/index.cshtml:34-38, 291-294, 305-308`
- `/Services/TranslationService.cs:438-441, 800-803`

### 問題 14: 配置管理頁面後端篩選 API 實作
**需求**: 修改 ConfigController.cs 以支援前端的管理員列表篩選功能，包含關鍵字搜尋、角色篩選、狀態篩選，以及正確的分頁資訊回傳。

**解決方案**:

**步驟1: 創建篩選 DTO 類別**
```csharp
// DTOs/UserDtos.cs
/// <summary>
/// 管理員篩選請求的資料傳輸物件
/// </summary>
public class AdminFilterDto
{
    /// <summary>
    /// 頁碼（從1開始）
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 篩選條件
    /// </summary>
    public AdminFilters? Filters { get; set; }
}

/// <summary>
/// 管理員篩選條件
/// </summary>
public class AdminFilters
{
    /// <summary>
    /// 關鍵字搜尋（用戶名稱、信箱、顯示名稱）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 超級管理員篩選（true=僅超級管理員, false=僅一般管理員, null=全部）
    /// </summary>
    public bool? SuperAdmin { get; set; }

    /// <summary>
    /// 狀態篩選（true=已啟用, false=未啟用, null=全部）
    /// </summary>
    public bool? Status { get; set; }
}
```

**步驟2: 修改 ConfigController 的 GetAllAdminAsync 方法**
```csharp
[HttpPost("")]
public async Task<IActionResult> GetAllAdminAsync([FromBody] AdminFilterDto dto)
{
    try
    {
        // 修正分頁參數名稱 (前端傳 page，後端期望 pages)
        var pageNumber = dto.Page > 0 ? dto.Page : 1;
        var pageSize = dto.PageSize > 0 ? dto.PageSize : 10;

        // 獲取篩選後的管理員資料
        var (admins, totalCount) = await GetFilteredAdminsAsync(pageNumber, pageSize, dto.Filters);

        // 計算分頁資訊
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new
        {
            success = true,
            data = admins,
            totalPages = totalPages,
            totalCount = totalCount,
            currentPage = pageNumber,
            pageSize = pageSize
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "獲取管理員列表失敗");
        return StatusCode(500, new
        {
            success = false,
            message = "獲取管理員列表失敗"
        });
    }
}
```

**步驟3: 實作篩選邏輯的私有方法**
```csharp
/// <summary>
/// 獲取篩選後的管理員資料
/// </summary>
private async Task<(List<object> Admins, int TotalCount)> GetFilteredAdminsAsync(int page, int pageSize, AdminFilters? filters)
{
    // 獲取所有管理員資料（角色 1 和 2）
    var query = await _userService.GetAllUsersQueryableAsync();
    
    // 篩選管理員（角色 >= 1）
    query = query.Where(u => u.Role >= 1);

    // 應用篩選條件
    if (filters != null)
    {
        // 關鍵字搜尋（使用者名稱或信箱）
        if (!string.IsNullOrWhiteSpace(filters.Keyword))
        {
            var keyword = filters.Keyword.Trim().ToLower();
            query = query.Where(u => 
                u.UserName.ToLower().Contains(keyword) || 
                u.Email.ToLower().Contains(keyword) ||
                u.DisplayName.ToLower().Contains(keyword));
        }

        // 超級管理員篩選
        if (filters.SuperAdmin.HasValue)
        {
            if (filters.SuperAdmin.Value)
            {
                query = query.Where(u => u.Role == 2); // 僅超級管理員
            }
            else
            {
                query = query.Where(u => u.Role == 1); // 僅一般管理員
            }
        }

        // 狀態篩選
        if (filters.Status.HasValue)
        {
            if (filters.Status.Value)
            {
                query = query.Where(u => u.Status == 1); // 已啟用
            }
            else
            {
                query = query.Where(u => u.Status == 0); // 未啟用
            }
        }
    }

    // 計算總數和分頁
    var totalCount = await _userService.CountUsersAsync(query);
    var users = await _userService.GetPagedUsersAsync(query, page, pageSize);

    // 轉換為前端需要的格式
    var adminList = users.Select(u => new
    {
        userId = u.UserId,
        userName = u.UserName,
        displayName = u.DisplayName ?? u.UserName,
        email = u.Email,
        avatarPath = u.AvatarPath,
        role = u.Role,
        status = u.Status,
        createTime = u.CreateTime,
        lastLoginTime = u.LastLoginTime
    }).ToList<object>();

    return (adminList, totalCount);
}
```

**前端與後端參數對應**:
- **前端篩選參數**：
  - `Config_AdminList_Keyword` → `filters.keyword`
  - `Config_AdminList_SuperAdmin` (1/2/0) → `filters.superAdmin` (true/false/null)
  - `Config_AdminList_Status` (1/2/0) → `filters.status` (true/false/null)

- **分頁參數修正**：
  - 前端發送 `page`，後端統一使用 `Page` 屬性

- **回傳格式增強**：
  - 增加 `totalPages`, `totalCount`, `currentPage` 分頁資訊
  - 支援前端分頁控制器正確顯示

**關鍵特性**:
1. **多條件篩選**: 支援關鍵字、角色、狀態的組合篩選
2. **模糊搜尋**: 關鍵字可搜尋用戶名稱、信箱、顯示名稱
3. **正確分頁**: 提供完整分頁資訊供前端使用
4. **錯誤處理**: 完整的異常處理和日誌記錄
5. **資料轉換**: 統一的前端資料格式

**相關檔案**:
- `/Areas/Dashboard/Controllers/Api/ConfigController.cs:82-135`
- `/DTOs/UserDtos.cs:198-238`

### 問題 13: ConfigController 中 Update 和 Delete 方法的 TODO 實作
**症狀**: ConfigController 中的 UpdateAdminAsync 和 DeleteAdminAsync 方法只有 TODO 註解，缺少實際實作

**原因**: 初始開發時只實作了篩選和創建功能，更新和刪除功能留下了 TODO 標記

**解決方案**: 實作完整的管理員更新和軟刪除功能

**UpdateAdminAsync 實作**:
```csharp
try
{
    // 獲取原始用戶實體以進行更新
    var userEntity = await _userService.GetUserEntityAsync(id);
    if (userEntity == null)
    {
        return NotFound(new { success = false, message = "找不到指定的用戶" });
    }

    // 更新用戶資料
    bool hasUpdates = false;

    if (model.Role.HasValue && model.Role.Value != userEntity.Role)
    {
        userEntity.Role = model.Role.Value;
        hasUpdates = true;
        _logger.LogInformation("更新用戶角色: {UserId} 從 {OldRole} 到 {NewRole}", 
            id, targetUser.Role, model.Role.Value);
    }

    if (model.Status.HasValue && model.Status.Value != userEntity.Status)
    {
        userEntity.Status = model.Status.Value;
        hasUpdates = true;
    }

    if (!hasUpdates)
    {
        return BadRequest(new { success = false, message = "沒有需要更新的資料" });
    }

    // 執行更新
    var updateResult = await _userService.UpdateUserEntityAsync(userEntity);
    if (!updateResult)
    {
        return StatusCode(500, new { success = false, message = "更新管理員資料失敗" });
    }

    return Ok(new { success = true, message = "管理員資料更新成功" });
}
catch (Exception ex)
{
    _logger.LogError(ex, "更新管理員資料時發生錯誤: {UserId}", id);
    return StatusCode(500, new { success = false, message = "更新管理員資料失敗" });
}
```

**DeleteAdminAsync 實作**:
```csharp
try
{
    // 檢查是否嘗試刪除自己
    if (currentUser.Value.UserId == id)
    {
        return BadRequest(new { success = false, message = "不能刪除自己的帳號" });
    }

    // 使用軟刪除以保持資料完整性
    var deleteResult = await _userService.SoftDeleteUserAsync(id);
    if (!deleteResult)
    {
        _logger.LogError("軟刪除管理員失敗: {UserId}", id);
        return StatusCode(500, new { success = false, message = "刪除管理員失敗" });
    }

    _logger.LogInformation("管理員軟刪除成功: {UserId}, 操作者: {OperatorId}", 
        id, currentUser.Value.UserId);

    return Ok(new { success = true, message = "管理員刪除成功" });
}
catch (Exception ex)
{
    _logger.LogError(ex, "刪除管理員時發生錯誤: {UserId}", id);
    return StatusCode(500, new { success = false, message = "刪除管理員失敗" });
}
```

**關鍵特性**:
1. **實體層更新**: 使用 `UpdateUserEntityAsync` 直接操作 User 實體，提供更好的控制
2. **變更追蹤**: 檢查是否有實際需要更新的欄位，避免無意義的資料庫操作
3. **軟刪除**: 使用 `SoftDeleteUserAsync` 保持資料完整性，便於恢復和審計
4. **安全檢查**: 防止管理員刪除自己的帳號，避免系統無法管理的情況
5. **詳細日誌**: 記錄所有管理員操作，包括操作者資訊，便於審計追蹤
6. **權限驗證**: 在實作前已通過 `CanEditUserAsync` 和 `CanDeleteUserAsync` 權限檢查
7. **完整錯誤處理**: 包含 try-catch 和詳細的錯誤回應

**使用的服務方法**:
- `GetUserEntityAsync()`: 獲取完整的用戶實體以進行修改
- `UpdateUserEntityAsync()`: 更新用戶實體到資料庫
- `SoftDeleteUserAsync()`: 執行軟刪除操作

**相關檔案**:
- `/Areas/Dashboard/Controllers/Api/ConfigController.cs:299-367, 400-436`
- `/Services/Interfaces/IUserService.cs:120, 195`
- `/DTOs/UserDtos.cs:278-315` (UpdateAdminDto)

---

## Azure 部署相關問題

### 問題 15: Azure App Service 部署時 npm install 失敗錯誤
**症狀**: 在 Visual Studio 發布到 Azure App Service 時，出現 npm install 失敗錯誤：
```
npm error code ENOENT
npm error syscall open
npm error path D:\...\Matrix\package.json
npm error errno -4058
npm error enoent Could not read package.json
```

**原因**: Azure App Service 和 Visual Studio 的部署系統具有**自動檢測機制**，當發現以下文件時會自動認為這是 Node.js 專案並嘗試執行 `npm install`：
- `tailwind.config.js`
- `webpack.config.js`
- `gulpfile.js`
- `package.json` (如果存在)

但專案中只有 `tailwind.config.js` 而沒有 `package.json`，因此 npm install 會失敗。

**解決方案**:

**方法1: 移除觸發檔案並添加禁用屬性**
```bash
# 重命名 tailwind.config.js 以避免自動檢測
mv tailwind.config.js tailwind.config.js.bak
```

**方法2: 在 Matrix.csproj 中添加禁用屬性**
```xml
<PropertyGroup>
    <!-- 原有屬性... -->
    <!-- 明確禁用所有 npm 相關操作 -->
    <DisableNpmInstall>true</DisableNpmInstall>
    <EnableNodeJsIntegration>false</EnableNodeJsIntegration>
    <SkipNodeModules>true</SkipNodeModules>
</PropertyGroup>
```

**方法3: 在發布配置文件中禁用構建**
```xml
<!-- Properties/PublishProfiles/xxx.pubxml -->
<PropertyGroup>
    <!-- 原有屬性... -->
    <!-- 禁用 Node.js 相關的構建步驟 -->
    <SCM_DO_BUILD_DURING_DEPLOYMENT>false</SCM_DO_BUILD_DURING_DEPLOYMENT>
    <WEBSITE_NODE_DEFAULT_VERSION>~18</WEBSITE_NODE_DEFAULT_VERSION>
</PropertyGroup>
```

**方法4: 創建最小 package.json (如果需要保留 tailwind.config.js)**
```json
{
  "name": "matrix",
  "version": "1.0.0",
  "scripts": {
    "build": "echo 'No build needed'"
  },
  "dependencies": {}
}
```

**為什麼 CSS 不會失效**:
1. **SCSS 已編譯完成**: 編譯好的 CSS 文件存在於 `wwwroot/css/` 目錄
2. **LibMan 管理前端資源**: 使用 `libman.json` 管理 Vue、Chart.js 等，不依賴 npm
3. **部署時直接複製**: CSS 文件會直接部署到服務器，無需 npm 處理

**預防措施**:
- 開發時使用本地工具編譯 SCSS 和 Tailwind CSS
- 避免在 ASP.NET Core 專案根目錄放置 Node.js 配置文件
- 使用 CDN 版本的前端庫，或透過 LibMan 管理
- 如需 Node.js 工具鏈，考慮分離前後端專案結構

**驗證部署成功**:
```bash
# 清理並重新發布
dotnet clean
dotnet publish --configuration Release
```

**相關檔案**:
- `/Matrix.csproj:12-15` (禁用屬性)
- `/Properties/PublishProfiles/web3matrix - Web Deploy.pubxml:28-30` (發布配置)
- `/tailwind.config.js` → `/tailwind.config.js.bak` (重命名避免檢測)
- `/wwwroot/css/tailwind.css` (已編譯的樣式文件)

---

## 資料庫遷移相關問題

### 問題 16: SmartASP.NET 到 Azure SQL Server 的資料庫遷移流程
**需求**: 將 SmartASP.NET SQL Server 的資料完整遷移到 Azure SQL Server，包含所有資料表結構和資料內容。

**解決方案**: 使用 SSMS 匯入和匯出資料精靈進行跨雲端資料庫遷移

**步驟1: 連接設定**
```
來源資料庫 (SmartASP.NET):
- 資料來源: Microsoft OLE DB Driver for SQL Server
- 伺服器名稱: SQL1004.site4now.net
- 驗證: SQL Server 驗證
- 使用者名稱: db_abbdbb_matrixdb_admin
- 密碼: !@#2025MatrixDB
- 資料庫: db_abbdbb_matrixdb

目的地資料庫 (Azure SQL):
- 資料來源: Microsoft OLE DB Driver for SQL Server
- 伺服器名稱: azurematrix-1.database.windows.net
- 驗證: SQL Server 驗證
- 使用者名稱: rootAccount
- 密碼: !@#VueDB
- 資料庫: MatrixDB
```

**步驟2: 選擇遷移方式**
- 選擇「複製一個或多個資料表或檢視的資料」
- 這會同時複製資料表結構和所有資料內容

**步驟3: 處理資料型別相容性問題**
```
常見問題: datetime2 vs smalldatetime
來源: datetime2 (高精度)
目的地: smalldatetime (低精度)

解決方案:
1. 修改目的地資料表結構為 datetime2
2. 或在匯入精靈中編輯對應設定
3. 或選擇「卸除並重新建立目的地資料表」
```

**步驟4: 執行選項**
- 選擇「立即執行」進行一次性遷移
- SSIS 封裝適合需要重複執行的情況

**注意事項**:
1. 確保兩端防火牆都允許連接
2. 大型資料表可能需要分批遷移
3. 檢查外鍵約束和索引是否正確遷移
4. 驗證遷移後的資料完整性

**替代方案**:
- 使用備份與還原（適合相同版本）
- 使用 SQL Server Data Tools (SSDT)
- 使用 Azure Database Migration Service

**相關連線字串**:
```csharp
// appsettings.Production.json 中的連線設定
"DefaultConnection": "Server=SQL1004.site4now.net;Database=db_abbdbb_matrixdb;..."
"AzureConnection": "Server=azurematrix-1.database.windows.net;Database=MatrixDB;..."
```

**相關檔案**:
- `/appsettings.Production.json:10-11` (連線字串設定)
- `/Program.cs:44` (連線字串選擇邏輯)

---

## 待補充問題
*後續遇到的問題和解決方案會持續更新到此處*

---

**最後更新**: 2025-08-24  
**版本**: v1.13
