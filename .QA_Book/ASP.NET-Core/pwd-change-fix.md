# 問題 24: 忘記密碼後修改密碼導致登入失敗

**症狀**: 
- 用戶使用忘記密碼功能獲得 token 並成功登入
- 用戶在個人頁面修改密碼成功
- 用戶再次登入時，/api/login 回傳 400 錯誤，無法登入

**原因**: 
在 `UserService.UpdatePersonProfileAsync` 方法中，當用戶修改密碼時：
1. 新密碼被正確雜湊並更新到 `User.Password`
2. 但是 `User.ForgotPwdToken` 沒有被清除
3. 登入驗證時，系統優先檢查 `ForgotPwdToken`，但舊 token 已無效，導致驗證失敗

**錯誤寫法**:
```csharp
// UserService.cs:757-760 (修復前)
if (!string.IsNullOrEmpty(dto.Password))
{
    user.Password = _passwordHasher.HashPassword(user, dto.Password);
    // ❌ 沒有清除 ForgotPwdToken
}
```

**正確寫法**:
```csharp
// UserService.cs:757-763 (修復後)
if (!string.IsNullOrEmpty(dto.Password))
{
    user.Password = _passwordHasher.HashPassword(user, dto.Password);
    // ✅ 清除忘記密碼 token，防止舊 token 干擾登入
    user.ForgotPwdToken = null;
    Console.WriteLine($"UpdatePersonProfileAsync: 密碼已更新並清除 ForgotPwdToken for user {user.UserName}");
}
```

**解決方案**: 
1. **清除 ForgotPwdToken**：在修改密碼時將 `user.ForgotPwdToken` 設為 null
2. **保存 User 實體變更**：確保 User 實體的變更被正確保存到資料庫

```csharp
// UserService.cs:769-775 (新增)
// 保存 User 變更（如果有密碼或Email變更）
if (!string.IsNullOrEmpty(dto.Password) || !string.IsNullOrEmpty(dto.Email))
{
    await _userRepository.UpdateAsync(user);
    await _userRepository.SaveChangesAsync();
    Console.WriteLine($"UpdatePersonProfileAsync: User 資料已保存");
}
```

**驗證邏輯**:
在 `UserRepository.ValidateUserAsync` 中，系統按以下順序驗證：
1. 首先檢查 `ForgotPwdToken` 是否存在並有效
2. 如果 token 驗證成功，清除 token 並允許登入
3. 如果沒有 token 或 token 無效，則使用正常密碼驗證

**相關檔案**: 
- `Services/UserService.cs:757-775` - 密碼修改邏輯
- `Repository/UserRepository.cs:86-101` - Token 優先驗證邏輯
- `Controllers/Api/ProfileController.cs:127-156` - 個人資料更新 API

**測試步驟**:
1. 使用忘記密碼功能獲得 token
2. 使用 token 成功登入
3. 修改密碼
4. 登出後重新登入，應該能使用新密碼成功登入