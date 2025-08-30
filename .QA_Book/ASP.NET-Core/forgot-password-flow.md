# 問題：如何實現忘記密碼後的安全密碼重設流程？

**症狀**: 
應用程式需要一個功能，讓忘記密碼的用戶可以安全地重設密碼。流程應包含發送臨時登入憑證，並在用戶設定新密碼後使其失效。

**原因**: 
原始的身份驗證流程缺乏密碼重設機制。當用戶忘記密碼時，沒有任何方式可以重新獲得帳戶存取權限。因此，需要引入一個使用臨時 Token 的流程，讓用戶能暫時登入並更新其密碼。

**解決方案**: 
整個流程的核心是引入一個臨時的 `ForgotPwdToken`，並調整驗證邏輯來應對這種臨時登入情境。

1.  **Token 的產生與儲存**:
    - 當用戶在「忘記密碼」頁面請求重設時，後端會產生一個安全的臨時性 Token。
    - 這個 Token 經過雜湊處理後，儲存在 `User` model 的 `ForgotPwdToken` 欄位中。

2.  **臨時登入驗證**:
    - `UserRepository` 中的 `ValidateUserAsync` 方法被修改，以支援 Token 驗證。
    - 驗證時，系統會優先檢查 `ForgotPwdToken` 是否存在且匹配。如果匹配成功，用戶即可登入。
    - **重要設計**: 僅使用 Token 成功登入後，`ForgotPwdToken` **不會**被清除。這個設計允許用戶在實際修改密碼前，若不小心登出或連線中斷，仍能使用同一個 Token 重新登入。

3.  **密碼更新與 Token 失效**:
    - 用戶使用臨時 Token 登入後，會被引導至個人資料頁面更新密碼。
    - 在 `UserService.UpdatePersonProfileAsync`（或類似的服務）中，當用戶提交新密碼並成功更新後，系統必須呼叫 `IUserRepository.ClearForgotPasswordTokenAsync` 方法。
    - `ClearForgotPasswordTokenAsync` 會將 `ForgotPwdToken` 欄位設為 `null`，從而永久性地使該臨時 Token 失效，確保帳戶安全。

**正確寫法** (在 `UserRepository.cs` 中的驗證邏輯):
```csharp
// ... in ValidateUserAsync ...

// 首先檢查是否使用 ForgotPwdToken 登入
if (!string.IsNullOrEmpty(user.ForgotPwdToken))
{
    var tokenResult = _passwordHasher.VerifyHashedPassword(user, user.ForgotPwdToken, password);
    if (tokenResult == PasswordVerificationResult.Success)
    {
        // 成功使用 Token 登入
        // 不要在這裡清除 token，讓用戶可以重複使用 token 登入
        // token 只會在用戶修改密碼時被清除 (例如在 UserService.UpdatePersonProfileAsync 中)
        return true;
    }
}

// ... 接著是正常的密碼驗證邏輯 ...
```

**相關檔案**: 
- `DTOs/PersonDtos.cs`
- `Repository/Interfaces/IUserRepository.cs`
- `Repository/UserRepository.cs`
- `Services/UserService.cs` (推斷)
- `Controllers/Api/ProfileController.cs` (推斷)
