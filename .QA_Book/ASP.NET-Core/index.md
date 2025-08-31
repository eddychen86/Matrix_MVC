# ASP.NET Core 問題目錄

本目錄包含 ASP.NET Core 後端相關的問題解決方案。

## 📋 問題清單

### 問題 2: 無法取得 Claims 資訊進行調試
**檔案**: [`claims-dbg.md`](./claims-dbg.md)  
**描述**: 在 Controller 中調試和記錄 Claims 資訊的方法  
**關鍵字**: Claims, 調試, Logger, Controller  
**相關檔案**: `Controller.cs`

### 問題 3: 篩選特定類型的 Claims
**檔案**: [`claims-flt.md`](./claims-flt.md)  
**描述**: 使用 LINQ 篩選和查詢特定類型的 Claims  
**關鍵字**: Claims, 篩選, LINQ, Role  
**相關檔案**: `Controller.cs`

### 問題 8: 實作管理員註冊服務與一般用戶註冊的差異化處理
**檔案**: [`admin-reg.md`](./admin-reg.md)  
**描述**: 建立獨立的管理員註冊流程與驗證邏輯  
**關鍵字**: 管理員註冊, 用戶註冊, Role, Status, 驗證  
**相關檔案**: `AdminRegistrationService.cs`, `RegisterController.cs`

### 問題 9: AdminRegistrationService 編譯錯誤修正
**檔案**: [`admin-cmp.md`](./admin-cmp.md)  
**描述**: 修正 AdminRegistrationService 的編譯錯誤  
**關鍵字**: 編譯錯誤, Status屬性, 依賴注入, 介面  
**相關檔案**: `AdminRegistrationService.cs:102`, `Program.cs:102`

### 問題 10: AdminRegistrationService 介面實作錯誤修復
**檔案**: [`admin-int.md`](./admin-int.md)  
**描述**: 修復介面定義重複和方法簽名不符的問題  
**關鍵字**: 介面實作, 重複定義, 方法簽名, 參數  
**相關檔案**: `IAdminRegistrationService.cs`, `AdminRegistrationService.cs`

### 問題 13: ConfigController 中 Update 和 Delete 方法的 TODO 實作
**檔案**: [`ctrl-todo.md`](./ctrl-todo.md)  
**描述**: 實作 ConfigController 的管理員更新和軟刪除功能  
**關鍵字**: ConfigController, Update, Delete, 軟刪除, 權限檢查  
**相關檔案**: `ConfigController.cs:299-367,400-436`

### 問題 14: 配置管理頁面後端篩選 API 實作
**檔案**: [`api-filt.md`](./api-filt.md)  
**描述**: 實作支援篩選條件的管理員列表 API  
**關鍵字**: API, 篩選, 分頁, DTO, LINQ查詢  
**相關檔案**: `ConfigController.cs:82-135`, `UserDtos.cs:198-238`

### 問題 15: Vue 3 Profile 頁面載入狀態管理與錯誤處理
**檔案**: [`prof-load.md`](./prof-load.md)  
**描述**: 為 Profile 頁面加入載入狀態和錯誤處理機制  
**關鍵字**: 載入狀態, 錯誤處理, HTTP狀態碼, 重試機制  
**相關檔案**: `profile.js:777-827`, `Profile/Index.cshtml:3-16`

### 問題 19: 實現基於 Token 的忘記密碼功能
**檔案**: [`pwd-reset.md`](./pwd-reset.md)  
**描述**: 使用 token 方式實現安全的密碼重置功能，包含郵件發送、token 管理和登入驗證  
**關鍵字**: 忘記密碼, Token, 郵件發送, PasswordHasher, 安全驗證  
**相關檔案**: `ForgotPasswordController.cs`, `UserService.cs`, `UserRepository.cs`, `GmailService.cs`

### 問題 20: 未登入用戶訪問頁面自動重定向到登入頁
**檔案**: [`auth-redirect.md`](./auth-redirect.md)  
**描述**: 解決自定義授權屬性導致未登入用戶無法訪問特定頁面的問題  
**關鍵字**: 授權, 重定向, MemberAuthorization, ActionFilter, 訪客存取  
**相關檔案**: `Controllers/ProfileController.cs:6`, `Attributes/RoleAuthorizationAttribute.cs:33-46`

### 問題 21: macOS 上的 NuGet 包路徑解析錯誤
**檔案**: [`nuget-path.md`](./nuget-path.md)  
**描述**: 修復 macOS 上因 NuGet 配置錯誤導致的 Windows 路徑解析問題  
**關鍵字**: NuGet, 路徑錯誤, macOS, 跨平台, 包源配置  
**相關檔案**: `~/.nuget/NuGet/NuGet.Config`

### 問題 22: ViewComponent 中引用使用者資訊 (DisplayName 和 AvatarPath)
**檔案**: [`vw-comp-usr.md`](./vw-comp-usr.md)  
**描述**: 在 ViewComponent 的 View 中正確顯示使用者的顯示名稱和頭像路徑  
**關鍵字**: ViewComponent, 使用者資訊, Claims, ViewModel, Razor  
**相關檔案**: `ViewComponents/CreatePostPopupViewComponent.cs:9-20`, `ViewModels/MenuViewModel.cs:24-28`

### 問題 24: 忘記密碼後修改密碼導致登入失敗
**檔案**: [`pwd-change-fix.md`](./pwd-change-fix.md)  
**描述**: 修復用戶使用忘記密碼 token 登入後修改密碼時未清除 ForgotPwdToken 導致的登入問題  
**關鍵字**: 忘記密碼, Token 清除, 密碼修改, 登入驗證, ForgotPwdToken  
**相關檔案**: `UserService.cs:757-775`, `UserRepository.cs:86-101`, `ProfileController.cs:127-156`

### 問題 25: 忘記密碼流程的 Token 管理策略
**檔案**: [`forgot-password-flow.md`](./forgot-password-flow.md)
**描述**: 實現忘記密碼功能時，關於臨時 Token 的登入驗證與失效時機的設計策略。
**關鍵字**: 忘記密碼, Token 管理, 安全流程, ValidateUserAsync, ClearForgotPasswordTokenAsync
**相關檔案**: `UserRepository.cs`, `IUserRepository.cs`, `UserService.cs`

### 問題 26: Razor 頁面中 Vue.js @error 事件語法編譯錯誤
**檔案**: [`razor-vue-error.md`](./razor-vue-error.md)
**描述**: 修復 Razor 頁面中使用 Vue.js 事件處理器時的語法衝突編譯錯誤
**關鍵字**: Razor, Vue.js, @error, 編譯錯誤, 轉義語法, @@
**相關檔案**: `Views/Home/Index.cshtml:25,42`

### 問題 27: 共用圖片驗證服務與數據清理
**檔案**: [`img-validation-service.md`](./img-validation-service.md)
**描述**: 建立共用的圖片驗證服務，在 API 層檢查檔案存在性並清理無效路徑
**關鍵字**: 圖片驗證, 共用服務, 數據清理, 快取, 批量檢查, 性能優化
**相關檔案**: `Services/ImageValidationService.cs`, `Controllers/Api/PostController.cs:66-111`

### 問題 28: EnrichWithImageValidationAsync 可空性 CS8603 修復
**檔案**: [`nullable-cs8603-imgvalidation.md`](./nullable-cs8603-imgvalidation.md)
**描述**: 修正 `ImageValidationService` 的泛型方法在可空性啟用下回傳 null 的合約不一致問題
**關鍵字**: 可空性, CS8603, 泛型, 服務層, 介面合約
**相關檔案**: `Services/Interfaces/IImageValidationService.cs`, `Services/ImageValidationService.cs`

### 問題 29: ValidateImagesAsync 重複鍵導致 ToDictionary 例外 (500)
**檔案**: [`img-validation-dupkey.md`](./img-validation-dupkey.md)
**描述**: 批量驗證圖片時因重複路徑導致 `ToDictionary` 拋 `ArgumentException`，造成 500
**關鍵字**: ToDictionary, 重複鍵, Distinct, 快取, 服務層
**相關檔案**: `Services/ImageValidationService.cs`, `Controllers/Api/PostController.cs`

### 問題 30: CommonController Primary Constructor 注入 ILogger 造成 CS9105/CS9113
**檔案**: [`common-logger-static.md`](./common-logger-static.md)  
**描述**: 在 static 方法中使用 Primary Constructor 參數導致編譯錯誤，改以 HttpContext DI 解析 logger  
**關鍵字**: CS9105, CS9113, Primary Constructor, ILogger, static, DI  
**相關檔案**: `Controllers/CommonController.cs`

### 問題 31: CommonController 指向不存在的屬性導致 CS1061（auth.displayName）
**檔案**: [`menu-auth-displayname.md`](./menu-auth-displayname.md)  
**描述**: 大小寫錯誤與模型不一致導致 CS1061，移除無效欄位指派並使用 `UserName` 顯示  
**關鍵字**: CS1061, DisplayName, 駝峰/帕斯卡命名, MenuViewModel  
**相關檔案**: `Controllers/CommonController.cs`, `ViewModels/MenuViewModel.cs`, `Extensions/CookieExtension.cs`

### 問題 32: 使用者名稱 Substring 超出長度導致 ArgumentOutOfRangeException
**檔案**: [`menu-substring-oob.md`](./menu-substring-oob.md)  
**描述**: 字串長度判斷與截斷長度不一致，導致 6~7 字元時 Substring 越界  
**關鍵字**: ArgumentOutOfRangeException, Substring, 長度判斷, 邊界條件  
**相關檔案**: `Controllers/CommonController.cs`

---

## 🔍 快速搜尋

### 功能分類
- **身份驗證**: 問題 2, 3 (Claims 相關), 問題 20 (授權重定向)
- **使用者管理**: 問題 8, 9, 10 (管理員註冊)
- **API 開發**: 問題 13, 14 (Controller 實作)
- **錯誤處理**: 問題 15 (狀態管理)
- **安全功能**: 問題 19 (密碼重置), 問題 20 (權限控制), 問題 24 (Token 清除), 問題 25 (Token 管理)

### 技術領域
- **Controller**: 問題 2, 3, 13, 14, 20, 27
- **Service Layer**: 問題 8, 9, 10, 19, 24, 25, 27
- **依賴注入**: 問題 9, 10, 27
- **前後端整合**: 問題 14, 15, 26
- **授權機制**: 問題 20
- **Razor 語法**: 問題 26
- **性能優化**: 問題 27

### 難度分級
- **初級**: 問題 2, 3 (Claims 調試), 問題 20 (授權設定), 問題 26 (Razor 語法)
- **中級**: 問題 8, 13, 14 (業務邏輯), 問題 19 (安全功能), 問題 24 (Token 管理), 問題 25 (安全流程), 問題 27 (圖片驗證)
- **高級**: 問題 9, 10, 15 (架構設計)

## 📊 統計

- **總問題數**: 21
- **已解決**: 21
- **Controller 相關**: 10
- **Service 相關**: 9
- **ViewComponent 相關**: 1
- **錯誤修復**: 9
- **安全功能**: 4
- **授權機制**: 1
- **跨平台問題**: 1
- **Razor 語法**: 1
- **性能優化**: 1
- **最後更新**: 2025-08-31

## 🔍 快速搜尋

### 功能分類
- **身份驗證**: 問題 2, 3 (Claims 相關), 問題 20 (授權重定向)
- **使用者管理**: 問題 8, 9, 10 (管理員註冊)
- **API 開發**: 問題 13, 14 (Controller 實作)
- **錯誤處理**: 問題 15 (狀態管理)
- **安全功能**: 問題 19 (密碼重置), 問題 20 (權限控制), 問題 24 (Token 清除)

### 技術領域
- **Controller**: 問題 2, 3, 13, 14, 20
- **Service Layer**: 問題 8, 9, 10, 19, 24
- **依賴注入**: 問題 9, 10
- **前後端整合**: 問題 14, 15
- **授權機制**: 問題 20

### 難度分級
- **初級**: 問題 2, 3 (Claims 調試), 問題 20 (授權設定)
- **中級**: 問題 8, 13, 14 (業務邏輯), 問題 19 (安全功能), 問題 24 (Token 管理)
- **高級**: 問題 9, 10, 15 (架構設計)

## 📊 統計

- **總問題數**: 13
- **已解決**: 13
- **Controller 相關**: 6
- **Service 相關**: 5
- **ViewComponent 相關**: 1
- **錯誤修復**: 5
- **安全功能**: 3
- **授權機制**: 1
- **跨平台問題**: 1
- **最後更新**: 2025-08-30
