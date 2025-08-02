# Person 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Person 表的主鍵 `PersonId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**外鍵依賴：** `UserId` 欄位必須參照已存在的 Users.UserId，不可自己亂寫！

## 表格結構說明
Person 表存儲用戶的詳細個人資料，與 User 表形成 1:1 關聯。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| PersonId | Guid | 個人資料唯一識別碼 (PK) | 是 | **自動生成** |
| UserId | Guid | 關聯用戶ID (FK) | 是 | - |
| DisplayName | string(50) | 顯示名稱 | 否 | NULL |
| Bio | string(300) | 個人簡介 | 否 | NULL |
| AvatarPath | string(2048) | 頭像檔案路徑 | 否 | NULL |
| BannerPath | string(2048) | 橫幅檔案路徑 | 否 | NULL |
| ExternalUrl | string(2048) | 外部網站連結 | 否 | NULL |
| IsPrivate | int | 隱私設定 (0=公開) | 是 | 0 |
| WalletAddress | string | 區塊鏈錢包地址 | 否 | NULL |
| ModifyTime | datetime | 最後修改時間 | 否 | NULL |

## 🔧 假資料匯入範例

### 匯入前準備：確認 Users 表資料
```sql
-- 必須先查詢 Users 表取得有效的 UserId
SELECT UserId, UserName FROM Users;
```
UserId                               | UserName
-------------------------------------|------------
12345678-1234-1234-1234-123456789abc | alice_chen
12345678-1234-1234-1234-123456789def | bob_wang
12345678-1234-1234-1234-123456789ghi | admin_user

### 匯入前資料庫狀態
```sql
SELECT * FROM Persons;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Users 表查詢到的真實 UserId
INSERT INTO Persons (UserId, DisplayName, Bio, AvatarPath, IsPrivate, WalletAddress, ModifyTime)
VALUES 
    ('12345678-1234-1234-1234-123456789abc', 'Alice Chen', 'Web3 愛好者，專注於 DeFi 技術研究', '/uploads/avatars/alice.jpg', 0, '0x1234567890abcdef', GETDATE()),
    ('12345678-1234-1234-1234-123456789def', 'Bob Wang', '區塊鏈開發者，致力於智能合約開發', '/uploads/avatars/bob.jpg', 0, '0xabcdef1234567890', GETDATE()),
    ('12345678-1234-1234-1234-123456789ghi', 'Matrix Admin', '平台管理員', '/uploads/avatars/admin.jpg', 1, NULL, GETDATE());
```

### 匯入後資料庫狀態
```sql
SELECT PersonId, UserId, DisplayName, Bio, IsPrivate, WalletAddress FROM Persons;
```
PersonId                             | UserId                               | DisplayName  | Bio                           | IsPrivate | WalletAddress
-------------------------------------|--------------------------------------|-------------|-------------------------------|-----------|-------------------
98765432-1234-1234-1234-123456789abc | 12345678-1234-1234-1234-123456789abc | Alice Chen  | Web3 愛好者，專注於 DeFi 技術研究 | 0         | 0x1234567890abcdef
98765432-1234-1234-1234-123456789def | 12345678-1234-1234-1234-123456789def | Bob Wang    | 區塊鏈開發者，致力於智能合約開發  | 0         | 0xabcdef1234567890
98765432-1234-1234-1234-123456789ghi | 12345678-1234-1234-1234-123456789ghi | Matrix Admin| 平台管理員                     | 1         | NULL

## 📋 匯入注意事項

### 外鍵依賴檢查
```sql
-- 匯入前必須先確認 User 表中存在對應的 UserId
SELECT UserId FROM Users WHERE UserId IN ('要匯入的UserId列表');
```

### 必要欄位檢查
- `UserId`: 必須是 Users 表中已存在的 UserId
- `IsPrivate`: 0=公開，1=私人

### 資料完整性規則
- `DisplayName`: 最大長度 50 字元
- `Bio`: 最大長度 300 字元
- `AvatarPath`, `BannerPath`, `ExternalUrl`: 最大長度 2048 字元
- `WalletAddress`: Web3 錢包地址格式

### 關聯影響
Person 記錄與 User 是 1:1 關聯：
- 每個 User 只能有一個對應的 Person 記錄
- 每個 Person 記錄必須對應一個有效的 User

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：使用不存在的 UserId
INSERT INTO Persons (UserId, DisplayName) 
VALUES ('99999999-9999-9999-9999-999999999999', 'Test User');
-- Error: FOREIGN KEY constraint failed
```

### ✅ 正確範例
```sql
-- 正確：先查詢再使用
DECLARE @UserId GUID = (SELECT TOP 1 UserId FROM Users WHERE UserName = 'alice_chen');
INSERT INTO Persons (UserId, DisplayName) VALUES (@UserId, 'Alice Chen');
```

### 匯入順序建議
1. 先確保 Users 表有資料
2. 查詢 Users 表取得有效的 UserId
3. 使用查詢到的 UserId 來插入 Person 資料
4. 使用 GETDATE() 設定 ModifyTime