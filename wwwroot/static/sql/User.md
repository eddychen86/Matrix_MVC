# User 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** User 表的主鍵 `UserId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

## 表格結構說明
User 表是系統的用戶帳號基礎表，存儲用戶的登入資訊和基本設定。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| UserId | Guid | 用戶唯一識別碼 (PK) | 是 | **自動生成** |
| Role | int | 權限等級 (0=一般用戶) | 是 | 0 |
| UserName | string(50) | 用戶顯示名稱 | 是 | - |
| Email | string(100) | 電子郵件地址 | 是 | - |
| Password | string | 加密密碼 | 是 | - |
| Country | string | 所在國家 | 否 | NULL |
| Gender | int | 性別 | 否 | NULL |
| CreateTime | datetime | 建立時間 | 是 | GETDATE() |
| LastLoginTime | datetime | 最後登入時間 | 否 | NULL |
| Status | int | 帳號狀態 (0=啟用) | 是 | 0 |

## 🔧 假資料匯入範例

### 匯入前資料庫狀態
```sql
SELECT * FROM Users;
-- (empty table)
```

### 匯入指令
```sql
-- 插入一般用戶假資料
INSERT INTO Users (Role, UserName, Email, Password, Country, Gender, CreateTime, Status)  
VALUES 
    (0, 'alice_chen', 'alice@example.com', 'hashed_password_1', 'Taiwan', 1, GETDATE(), 0),
    (0, 'bob_wang', 'bob@example.com', 'hashed_password_2', 'Japan', 0, GETDATE(), 0),
    (1, 'admin_user', 'admin@matrix.com', 'hashed_admin_password', 'Taiwan', NULL, GETDATE(), 0);
```

### 匯入後資料庫狀態
```sql
SELECT UserId, Role, UserName, Email, Country, Gender, CreateTime, Status FROM Users;
```
UserId                               | Role | UserName   | Email              | Country | Gender | CreateTime          | Status
-------------------------------------|------|------------|-------------------|---------|--------|--------------------|---------
12345678-1234-1234-1234-123456789abc | 0    | alice_chen | alice@example.com  | Taiwan  | 1      | 2024-01-01 10:00:00| 0
12345678-1234-1234-1234-123456789def | 0    | bob_wang   | bob@example.com    | Japan   | 0      | 2024-01-01 10:00:01| 0  
12345678-1234-1234-1234-123456789ghi | 1    | admin_user | admin@matrix.com   | Taiwan  | NULL   | 2024-01-01 10:00:02| 0

## 📋 匯入注意事項

### 必要欄位檢查
- `UserName`: 不可為空，最大長度 50 字元
- `Email`: 必須符合電子郵件格式，最大長度 100 字元
- `Password`: 應使用加密後的密碼，不可為明碼

### 資料完整性規則
- `Email` 應該是唯一的（雖然資料庫層面未強制，但業務邏輯應確保）
- `Role` 值：0=一般用戶，1=管理員
- `Status` 值：0=啟用，1=停用，2=被封禁
- `Gender` 值：0=男性，1=女性，NULL=未指定

### 關聯影響
新增 User 後，通常需要：
1. 在 Person 表中建立對應的個人資料記錄
2. 確保 `Person.UserId` 外鍵正確對應到新建立的 `User.UserId`

## 🚨 常見錯誤避免
1. **不要手動設定 UserId**：讓 Entity Framework 自動生成
2. **密碼安全**：確保密碼已正確加密
3. **電子郵件格式**：使用有效的電子郵件格式
4. **字串長度**：注意 UserName 和 Email 的最大長度限制