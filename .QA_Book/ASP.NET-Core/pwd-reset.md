# 問題 19: 實現基於 Token 的忘記密碼功能

**症狀**: 用戶需要忘記密碼重置功能，要求使用 token 方式而不是直接重置密碼

**需求**: 
- 發送包含臨時 token 的郵件而不是明文密碼
- token 加密存儲在資料庫中
- 用戶可使用 token 登入，登入後 token 自動失效
- token 符合系統密碼規則以便直接作為臨時密碼使用

---

## 🔧 解決方案

### 1. 添加 ForgotPwdToken 字段到 User 模型

**檔案**: `Models/User.cs`

```csharp
/// <summary>
/// 忘記密碼令牌（加密存儲）
/// </summary>
public string? ForgotPwdToken { get; set; }
```

### 2. 創建忘記密碼 API Controller

**檔案**: `Controllers/Api/ForgotPasswordController.cs`

```csharp
[Route("api/[controller]")]
[ApiController]
public class ForgotPasswordController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordRequest request)
    {
        // 生成臨時 token
        string temporaryToken = GenerateTemporaryToken();
        
        // 存儲加密的 token 到資料庫
        var tokenResult = await _userService.SetForgotPasswordTokenAsync(request.Email, temporaryToken);
        
        if (!tokenResult)
        {
            return Ok(new { success = true, message = "如果該電子郵件已註冊，將會收到密碼重置郵件" });
        }
        
        // 發送郵件包含 token
        var emailSent = await _emailService.SendPasswordResetEmailAsync(
            request.Email, 
            request.Email.Split('@')[0],
            temporaryToken
        );
        
        return Ok(new { success = true, message = "密碼重置郵件已發送，請檢查您的郵箱" });
    }

    private string GenerateTemporaryToken()
    {
        // 生成符合密碼規則的8位 token：大小寫字母 + 數字 + 特殊字符
        var random = new Random();
        var token = new List<char>();
        
        const string lowercase = "abcdefghijkmnpqrstuvwxyz";
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string digits = "2345678";
        const string specialChars = "!@#$%^&*";
        
        // 確保每種字符類型都包含
        token.Add(lowercase[random.Next(lowercase.Length)]);
        token.Add(uppercase[random.Next(uppercase.Length)]);
        token.Add(digits[random.Next(digits.Length)]);
        token.Add(specialChars[random.Next(specialChars.Length)]);
        
        // 填充到8位並打亂順序
        const string allChars = lowercase + uppercase + digits + specialChars;
        for (int i = 4; i < 8; i++)
        {
            token.Add(allChars[random.Next(allChars.Length)]);
        }
        
        for (int i = token.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (token[i], token[j]) = (token[j], token[i]);
        }
        
        return new string(token.ToArray());
    }
}
```

### 3. 實現 UserService 的 token 管理方法

**檔案**: `Services/UserService.cs`

```csharp
public async Task<bool> SetForgotPasswordTokenAsync(string email, string token)
{
    var user = await _userRepository.GetByEmailAsync(email);
    
    if (user == null || user.IsDelete != 0)
    {
        return false;
    }

    // 加密並存儲 token
    user.ForgotPwdToken = _passwordHasher.HashPassword(user, token);
    
    await _userRepository.UpdateAsync(user);
    await _userRepository.SaveChangesAsync(); // 重要：必須調用 SaveChanges
    return true;
}
```

### 4. 修改登入驗證支持 token

**檔案**: `Repository/UserRepository.cs`

```csharp
public async Task<bool> ValidateUserAsync(string username, string password)
{
    var user = await _dbSet
        .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

    if (user == null || string.IsNullOrEmpty(user.Password)) return false;

    // 首先檢查是否使用 ForgotPwdToken 登入
    if (!string.IsNullOrEmpty(user.ForgotPwdToken))
    {
        var tokenResult = _passwordHasher.VerifyHashedPassword(user, user.ForgotPwdToken, password);
        if (tokenResult == PasswordVerificationResult.Success)
        {
            // 清空 ForgotPwdToken（token 只能使用一次）
            user.ForgotPwdToken = null;
            await this.UpdateAsync(user);
            await this.SaveChangesAsync();
            
            return true;
        }
    }

    // 繼續原來的密碼驗證邏輯...
}
```

### 5. 郵件模板更新

**檔案**: `Services/GmailService.cs`

郵件中的 `temporaryPassword` 現在是 token，用戶收到的是符合密碼規則的8位臨時密碼。

---

## 🚨 關鍵問題與解決

### 問題 1: Token 沒有存入資料庫
**原因**: 只調用了 `UpdateAsync()` 但沒有 `SaveChangesAsync()`
**解決**: 必須在 `UpdateAsync()` 後調用 `SaveChangesAsync()`

```csharp
await _userRepository.UpdateAsync(user);
await _userRepository.SaveChangesAsync(); // 必須加上這行
```

### 問題 2: 資料庫欄位不存在錯誤
**原因**: 模型添加了 `ForgotPwdToken` 但資料庫沒有對應欄位
**解決**: 創建並應用 Entity Framework 遷移

```bash
dotnet ef migrations add AddForgotPwdTokenToUser
dotnet ef database update
```

### 問題 3: 密碼驗證失敗
**原因**: 新舊密碼哈希系統兼容性問題
**解決**: Token 驗證優先於原密碼驗證，使用相同的 `PasswordHasher`

---

## 🔄 工作流程

1. **用戶請求重置** → 輸入 email 並提交忘記密碼表單
2. **生成 token** → 系統生成符合密碼規則的8位隨機 token
3. **加密存儲** → Token 使用 `PasswordHasher` 加密後存到 `ForgotPwdToken` 欄位
4. **發送郵件** → 將明文 token 通過郵件發送給用戶
5. **用戶登入** → 使用用戶名 + token 進行登入
6. **驗證與清除** → 系統驗證 token 成功後立即清空該欄位

---

## 🛡️ 安全特性

- **Token 一次性**: 使用後立即從資料庫清除
- **加密存儲**: Token 在資料庫中以哈希值存儲，不是明文
- **密碼規則**: Token 符合系統密碼複雜度要求
- **隱私保護**: 即使用戶不存在也返回成功訊息

---

## 📝 相關檔案

- `Models/User.cs:83` - ForgotPwdToken 字段定義
- `Controllers/Api/ForgotPasswordController.cs` - 忘記密碼 API
- `Services/UserService.cs:474-512` - Token 管理方法
- `Repository/UserRepository.cs:78-101` - Token 登入驗證
- `Services/GmailService.cs:75-133` - 郵件發送服務
- `Views/Auth/Login.cshtml:67-88` - 前端忘記密碼表單
- `wwwroot/js/auth/login-event.js:19-79` - 前端 JavaScript 處理

---

## 🎯 測試驗證

1. 提交忘記密碼表單，確認 token 存入資料庫
2. 檢查郵件中的8位 token 格式
3. 使用 token 登入，確認登入成功
4. 驗證登入後 `ForgotPwdToken` 欄位被清空
5. 嘗試重複使用已用過的 token，確認登入失敗