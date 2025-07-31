# Follow 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Follow 表的主鍵 `FollowId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**外鍵依賴：** 
- `UserId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！
- `FollowedId` 是靈活外鍵，可能指向不同實體，需依業務邏輯處理

## 表格結構說明
Follow 表記錄用戶的關注關係，支援關注用戶、主題標籤等不同類型的實體。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| FollowId | Guid | 關注記錄唯一識別碼 (PK) | 是 | **自動生成** |
| UserId | Guid | 關注者PersonId (FK) | 是 | - |
| FollowedId | Guid | 被關注對象Id | 是 | - |
| Type | int | 關注類型 | 是 | - |
| FollowTime | datetime | 關注時間 | 是 | GETDATE() |

### 關注類型說明
- `Type = 0`: 關注用戶（FollowedId 指向 Persons.PersonId）
- `Type = 1`: 關注標籤（FollowedId 指向 Hashtags.TagId）
- 其他類型可依業務需求擴展

## 🔧 假資料匯入範例

### 匯入前準備：確認相關表資料
```sql
-- 1. 查詢 Persons 表取得有效的 PersonId
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId;

-- 2. 查詢 Hashtags 表取得有效的 TagId (如果有的話)
SELECT TagId, Content FROM Hashtags WHERE Status = 0;
```

#### Persons 表查詢結果：
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789abc | Alice Chen
98765432-1234-1234-1234-123456789def | Bob Wang  
98765432-1234-1234-1234-123456789ghi | Matrix Admin

### 匯入前資料庫狀態
```sql
SELECT * FROM Follows;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從相關表查詢到的真實 ID
INSERT INTO Follows (UserId, FollowedId, Type, FollowTime)
VALUES 
    -- Alice 關注 Bob (Type = 0: 關注用戶)
    ('98765432-1234-1234-1234-123456789abc', 
     '98765432-1234-1234-1234-123456789def', 
     0, GETDATE()),
     
    -- Bob 關注 Alice (Type = 0: 關注用戶)
    ('98765432-1234-1234-1234-123456789def', 
     '98765432-1234-1234-1234-123456789abc', 
     0, GETDATE()),
     
    -- Admin 關注 Alice (Type = 0: 關注用戶)
    ('98765432-1234-1234-1234-123456789ghi', 
     '98765432-1234-1234-1234-123456789abc', 
     0, GETDATE());
     
-- 如果有標籤資料，可以加入標籤關注範例
-- ('98765432-1234-1234-1234-123456789abc', 
--  'tag-guid-from-hashtags-table', 
--  1, GETDATE());
```

### 匯入後資料庫狀態
```sql
SELECT FollowId, UserId, FollowedId, Type, FollowTime 
FROM Follows
ORDER BY FollowTime;
```
FollowId                             | UserId                               | FollowedId                           | Type | FollowTime
-------------------------------------|--------------------------------------|--------------------------------------|------|---------------------
hhhhhhhh-hhhh-hhhh-hhhh-hhhhhhhhhhhh | 98765432-1234-1234-1234-123456789abc | 98765432-1234-1234-1234-123456789def | 0    | 2024-01-01 13:00:00
iiiiiiii-iiii-iiii-iiii-iiiiiiiiiiii | 98765432-1234-1234-1234-123456789def | 98765432-1234-1234-1234-123456789abc | 0    | 2024-01-01 13:00:01
jjjjjjjj-jjjj-jjjj-jjjj-jjjjjjjjjjjj | 98765432-1234-1234-1234-123456789ghi | 98765432-1234-1234-1234-123456789abc | 0    | 2024-01-01 13:00:02

### 驗證匯入結果
```sql
-- 檢查關注類型分布
SELECT Type, 
       CASE Type WHEN 0 THEN '關注用戶' WHEN 1 THEN '關注標籤' ELSE '其他' END AS TypeName,
       COUNT(*) AS Count
FROM Follows 
GROUP BY Type;

-- 檢查用戶關注關係
SELECT 
    u1.DisplayName AS Follower,
    u2.DisplayName AS Followed,
    f.FollowTime
FROM Follows f
INNER JOIN Persons p1 ON f.UserId = p1.PersonId
INNER JOIN Users u1 ON p1.UserId = u1.UserId
INNER JOIN Persons p2 ON f.FollowedId = p2.PersonId
INNER JOIN Users u2 ON p2.UserId = u2.UserId
WHERE f.Type = 0;
```

## 📋 匯入注意事項

### 外鍵依賴檢查
```sql
-- 確認 UserId 存在於 Persons 表
SELECT COUNT(*) FROM Persons WHERE PersonId IN ('要匯入的UserId列表');

-- 如果 Type = 0，確認 FollowedId 存在於 Persons 表
SELECT COUNT(*) FROM Persons WHERE PersonId IN ('要匯入的FollowedId列表');

-- 如果 Type = 1，確認 FollowedId 存在於 Hashtags 表
SELECT COUNT(*) FROM Hashtags WHERE TagId IN ('要匯入的FollowedId列表');
```

### 必要欄位檢查
- `UserId`: 必須是 Persons 表中已存在的 PersonId
- `FollowedId`: 根據 Type 值對應不同表的 ID
- `Type`: 0=關注用戶，1=關注標籤

### 資料完整性規則
- 用戶不能關注自己（UserId != FollowedId when Type = 0）
- 同一用戶對同一對象的同一類型關注應該唯一
- `FollowTime`: 使用 GETDATE()

### 業務邏輯考量
- 關注關係是單向的，不自動建立互相關注
- 取消關注時應刪除對應記錄
- 可能需要隱私設定檢查

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：用戶關注自己
INSERT INTO Follows (UserId, FollowedId, Type) 
VALUES ('98765432-1234-1234-1234-123456789abc', 
        '98765432-1234-1234-1234-123456789abc', 0);
-- Business Logic Error: 用戶不能關注自己
```

### ❌ 錯誤的外鍵理解
```sql
-- 錯誤：使用 Users.UserId 而不是 Persons.PersonId
INSERT INTO Follows (UserId, FollowedId, Type) 
VALUES ('12345678-1234-1234-1234-123456789abc', -- 這是 Users.UserId
        '98765432-1234-1234-1234-123456789def', 0);
-- Error: FOREIGN KEY constraint failed
```

### ✅ 正確範例
```sql
-- 正確：Alice 關注 Bob
DECLARE @AlicePersonId GUID = (
    SELECT PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'alice_chen'
);
DECLARE @BobPersonId GUID = (
    SELECT PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'bob_wang'
);

INSERT INTO Follows (UserId, FollowedId, Type, FollowTime)
VALUES (@AlicePersonId, @BobPersonId, 0, GETDATE());
```

### 匯入順序建議
1. 先確保 Persons 表有資料
2. 如果要關注標籤，確保 Hashtags 表有資料
3. 查詢相關表取得有效的 ID
4. 確認不會建立自我關注關係
5. 使用 GETDATE() 設定 FollowTime

### 資料維護建議
```sql
-- 檢查關注關係的完整性
SELECT 
    COUNT(*) AS InvalidFollows
FROM Follows f
WHERE f.Type = 0 
  AND f.UserId = f.FollowedId;  -- 自我關注檢查

-- 清理無效的關注關係
DELETE FROM Follows 
WHERE Type = 0 AND UserId = FollowedId;
```