# Friendship 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Friendship 表的主鍵 `FriendshipId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**雙重外鍵依賴：** 
- `UserId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！
- `FriendId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！

## 表格結構說明
Friendship 表記錄用戶之間的好友關係，包含邀請、接受、拒絕、封鎖等狀態管理。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| FriendshipId | Guid | 好友關係唯一識別碼 (PK) | 是 | **自動生成** |
| UserId | Guid | 發起邀請者PersonId (FK) | 是 | - |
| FriendId | Guid | 接收邀請者PersonId (FK) | 是 | - |
| Status | enum | 好友關係狀態 | 是 | - |
| RequestDate | datetime | 邀請發送時間 | 是 | GETDATE() |

### 好友狀態說明（FriendshipStatus）
- `0 (Pending)`: 待確認
- `1 (Accepted)`: 已接受
- `2 (Declined)`: 已拒絕
- `3 (Blocked)`: 已封鎖

## 🔧 假資料匯入範例

### 匯入前準備：確認 Persons 表資料
```sql
-- 查詢 Persons 表取得有效的 PersonId
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId;
```

#### Persons 表查詢結果：
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789abc | Alice Chen
98765432-1234-1234-1234-123456789def | Bob Wang  
98765432-1234-1234-1234-123456789ghi | Matrix Admin

### 匯入前資料庫狀態
```sql
SELECT * FROM Friendships;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Persons 表查詢到的真實 PersonId
INSERT INTO Friendships (UserId, FriendId, Status, RequestDate)
VALUES 
    -- Alice 向 Bob 發送好友邀請 (Status = 0: Pending)
    ('98765432-1234-1234-1234-123456789abc', 
     '98765432-1234-1234-1234-123456789def', 
     0, GETDATE()),
     
    -- Bob 向 Admin 發送好友邀請並已被接受 (Status = 1: Accepted)
    ('98765432-1234-1234-1234-123456789def', 
     '98765432-1234-1234-1234-123456789ghi', 
     1, DATEADD(HOUR, -1, GETDATE())),
     
    -- Admin 向 Alice 發送邀請但被拒絕 (Status = 2: Declined)
    ('98765432-1234-1234-1234-123456789ghi', 
     '98765432-1234-1234-1234-123456789abc', 
     2, DATEADD(HOUR, -2, GETDATE()));
```

### 匯入後資料庫狀態
```sql
SELECT FriendshipId, UserId, FriendId, Status, RequestDate 
FROM Friendships
ORDER BY RequestDate;
```
FriendshipId                         | UserId                               | FriendId                             | Status | RequestDate
-------------------------------------|--------------------------------------|--------------------------------------|--------|---------------------
kkkkkkkk-kkkk-kkkk-kkkk-kkkkkkkkkkkk | 98765432-1234-1234-1234-123456789ghi | 98765432-1234-1234-1234-123456789abc | 2      | 2024-01-01 12:00:00
llllllll-llll-llll-llll-llllllllllll | 98765432-1234-1234-1234-123456789def | 98765432-1234-1234-1234-123456789ghi | 1      | 2024-01-01 13:00:00
mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm | 98765432-1234-1234-1234-123456789abc | 98765432-1234-1234-1234-123456789def | 0      | 2024-01-01 14:00:00

### 驗證匯入結果
```sql
-- 檢查好友狀態分布
SELECT Status, 
       CASE Status 
           WHEN 0 THEN 'Pending (待確認)'
           WHEN 1 THEN 'Accepted (已接受)'
           WHEN 2 THEN 'Declined (已拒絕)'
           WHEN 3 THEN 'Blocked (已封鎖)'
       END AS StatusName,
       COUNT(*) AS Count
FROM Friendships 
GROUP BY Status;

-- 檢查好友關係詳情
SELECT 
    requester.DisplayName AS Requester,
    recipient.DisplayName AS Recipient,
    CASE f.Status 
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Accepted'
        WHEN 2 THEN 'Declined'
        WHEN 3 THEN 'Blocked'
    END AS Status,
    f.RequestDate
FROM Friendships f
INNER JOIN Persons p1 ON f.UserId = p1.PersonId
INNER JOIN Users requester ON p1.UserId = requester.UserId
INNER JOIN Persons p2 ON f.FriendId = p2.PersonId
INNER JOIN Users recipient ON p2.UserId = recipient.UserId;
```

## 📋 匯入注意事項

### 雙重外鍵依賴檢查
```sql
-- 確認兩個 PersonId 都存在
SELECT 'Requester Check' AS CheckType, COUNT(*) AS Count 
FROM Persons WHERE PersonId IN ('要匯入的UserId列表')
UNION ALL
SELECT 'Recipient Check', COUNT(*) 
FROM Persons WHERE PersonId IN ('要匯入的FriendId列表');
```

### 必要欄位檢查
- `UserId`: 必須是 Persons 表中已存在的 PersonId
- `FriendId`: 必須是 Persons 表中已存在的 PersonId
- `Status`: 0-3 的整數值對應不同狀態

### 資料完整性規則
- 用戶不能與自己建立好友關係（UserId != FriendId）
- 同一對用戶只能有一個好友關係記錄
- `RequestDate`: 使用 GETDATE() 或指定時間

### 業務邏輯考量
- 好友關係是雙向的，但只需一條記錄
- 被拒絕或封鎖的關係可能需要特殊處理
- 狀態變更時可能需要通知機制

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：用戶與自己建立好友關係
INSERT INTO Friendships (UserId, FriendId, Status) 
VALUES ('98765432-1234-1234-1234-123456789abc', 
        '98765432-1234-1234-1234-123456789abc', 0);
-- Business Logic Error: 用戶不能與自己建立好友關係
```

### ❌ 重複好友關係
```sql
-- 錯誤：重複建立好友關係
INSERT INTO Friendships (UserId, FriendId, Status, RequestDate) 
VALUES 
    ('98765432-1234-1234-1234-123456789abc', 
     '98765432-1234-1234-1234-123456789def', 0, GETDATE()),
    ('98765432-1234-1234-1234-123456789def', 
     '98765432-1234-1234-1234-123456789abc', 1, GETDATE());
-- Business Logic Error: 同一對用戶只需一條好友關係記錄
```

### ✅ 正確範例
```sql
-- 正確：建立單向好友邀請
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

-- 確保不是自我好友關係
IF @AlicePersonId != @BobPersonId
BEGIN
    INSERT INTO Friendships (UserId, FriendId, Status, RequestDate)
    VALUES (@AlicePersonId, @BobPersonId, 0, GETDATE());
END
```

### 匯入順序建議
1. 先確保 Persons 表有足夠的資料
2. 查詢 Persons 表取得有效的 PersonId
3. 確認不會建立自我好友關係
4. 確認不會重複建立好友關係
5. 根據業務場景選擇適當的 Status
6. 使用 GETDATE() 設定 RequestDate

### 好友關係管理建議
```sql
-- 檢查是否存在重複好友關係（雙向）
SELECT 
    f1.UserId, f1.FriendId, COUNT(*) as DuplicateCount
FROM Friendships f1
INNER JOIN Friendships f2 ON (
    (f1.UserId = f2.FriendId AND f1.FriendId = f2.UserId) OR
    (f1.UserId = f2.UserId AND f1.FriendId = f2.FriendId AND f1.FriendshipId != f2.FriendshipId)
)
GROUP BY f1.UserId, f1.FriendId
HAVING COUNT(*) > 1;

-- 檢查自我好友關係
SELECT * FROM Friendships WHERE UserId = FriendId;
```

### 狀態更新範例
```sql
-- 接受好友邀請
UPDATE Friendships 
SET Status = 1  -- Accepted
WHERE UserId = @RequesterId 
  AND FriendId = @RecipientId 
  AND Status = 0;  -- Pending

-- 拒絕好友邀請
UPDATE Friendships 
SET Status = 2  -- Declined
WHERE UserId = @RequesterId 
  AND FriendId = @RecipientId 
  AND Status = 0;  -- Pending
```