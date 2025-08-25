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

---

## 🔍 快速搜尋

### 功能分類
- **身份驗證**: 問題 2, 3 (Claims 相關), 問題 20 (授權重定向)
- **使用者管理**: 問題 8, 9, 10 (管理員註冊)
- **API 開發**: 問題 13, 14 (Controller 實作)
- **錯誤處理**: 問題 15 (狀態管理)
- **安全功能**: 問題 19 (密碼重置), 問題 20 (權限控制)

### 技術領域
- **Controller**: 問題 2, 3, 13, 14, 20
- **Service Layer**: 問題 8, 9, 10, 19
- **依賴注入**: 問題 9, 10
- **前後端整合**: 問題 14, 15
- **授權機制**: 問題 20

### 難度分級
- **初級**: 問題 2, 3 (Claims 調試), 問題 20 (授權設定)
- **中級**: 問題 8, 13, 14 (業務邏輯), 問題 19 (安全功能)
- **高級**: 問題 9, 10, 15 (架構設計)

## 📊 統計

- **總問題數**: 10
- **已解決**: 10
- **Controller 相關**: 6
- **Service 相關**: 4
- **錯誤修復**: 3
- **安全功能**: 2
- **授權機制**: 1
- **最後更新**: 2025-08-25