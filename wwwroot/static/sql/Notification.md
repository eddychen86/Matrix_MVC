# Notification 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Notification 表的主鍵 `NotifyId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**雙重外鍵依賴：** 
- `GetId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！
- `SendId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！

## 表格結構說明
Notification 表記錄系統中的通知訊息，包括文章留言通知、用戶私信等。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| NotifyId | Guid | 通知唯一識別碼 (PK) | 是 | **自動生成** |
| GetId | Guid | 接收者PersonId (FK) | 是 | - |
| SendId | Guid | 發送者PersonId (FK) | 是 | - |
| Type | int | 通知類型 | 是 | - |
| IsRead | int | 閱讀狀態 (0=未讀) | 是 | 0 |
| SentTime | datetime | 發送時間 | 是 | GETDATE() |
| IsReadTime | datetime | 閱讀時間 | 否 | NULL |

### 通知類型說明
- `Type = 0`: 文章留言通知
- `Type = 1`: 用戶私信（未來功能）
- 其他類型可依業務需求擴展

### 閱讀狀態說明
- `IsRead = 0`: 未讀
- `IsRead = 1`: 已讀

## 🔧 假資料匯入範例

### 匯入前準備：確認 Persons 表資料
```sql
-- 查詢 Persons 表取得有效的 PersonId
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId;
```

#### Persons 表查詢結果：
```
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789abc | Alice Chen
98765432-1234-1234-1234-123456789def | Bob Wang  
98765432-1234-1234-1234-123456789ghi | Matrix Admin
```

### 匯入前資料庫狀態
```sql
SELECT * FROM Notifications;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Persons 表查詢到的真實 PersonId
INSERT INTO Notifications (GetId, SendId, Type, IsRead, SentTime, IsReadTime)
VALUES 
    -- Bob 回覆了 Alice 的文章，通知 Alice (Type = 0: 文章留言)
    ('98765432-1234-1234-1234-123456789abc',  -- Alice 接收
     '98765432-1234-1234-1234-123456789def',  -- Bob 發送
     0, 0, GETDATE(), NULL),
     
    -- Alice 回覆了 Bob 的文章，通知 Bob (Type = 0: 文章留言)
    ('98765432-1234-1234-1234-123456789def',  -- Bob 接收  
     '98765432-1234-1234-1234-123456789abc',  -- Alice 發送
     0, 1, DATEADD(MINUTE, -30, GETDATE()), DATEADD(MINUTE, -15, GETDATE())),
     
    -- Admin 回覆了 Alice 的文章，通知 Alice (Type = 0: 文章留言)
    ('98765432-1234-1234-1234-123456789abc',  -- Alice 接收
     '98765432-1234-1234-1234-123456789ghi',  -- Admin 發送
     0, 0, DATEADD(MINUTE, -10, GETDATE()), NULL);
```

### 匯入後資料庫狀態
```sql
SELECT NotifyId, GetId, SendId, Type, IsRead, SentTime, IsReadTime 
FROM Notifications
ORDER BY SentTime DESC;
```
NotifyId                             | GetId                                | SendId                               | Type | IsRead | SentTime            | IsReadTime
-------------------------------------|--------------------------------------|--------------------------------------|------|--------|--------------------|--------------
nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn | 98765432-1234-1234-1234-123456789abc | 98765432-1234-1234-1234-123456789ghi | 0    | 0      | 2024-01-01 14:50:00 | NULL
oooooooo-oooo-oooo-oooo-oooooooooooo | 98765432-1234-1234-1234-123456789abc | 98765432-1234-1234-1234-123456789def | 0    | 0      | 2024-01-01 15:00:00 | NULL
pppppppp-pppp-pppp-pppp-pppppppppppp | 98765432-1234-1234-1234-123456789def | 98765432-1234-1234-1234-123456789abc | 0    | 1      | 2024-01-01 14:30:00 | 2024-01-01 14:45:00

### 驗證匯入結果
```sql
-- 檢查通知類型分布
SELECT Type, 
       CASE Type 
           WHEN 0 THEN '文章留言'
           WHEN 1 THEN '用戶私信'
           ELSE '其他'
       END AS TypeName,
       COUNT(*) AS Count
FROM Notifications 
GROUP BY Type;

-- 檢查閱讀狀態分布
SELECT IsRead,
       CASE IsRead WHEN 0 THEN '未讀' WHEN 1 THEN '已讀' END AS ReadStatus,
       COUNT(*) AS Count
FROM Notifications 
GROUP BY IsRead;

-- 檢查通知詳情
SELECT 
    receiver.DisplayName AS Receiver,
    sender.DisplayName AS Sender,
    CASE n.Type WHEN 0 THEN '文章留言' WHEN 1 THEN '用戶私信' END AS NotificationType,
    CASE n.IsRead WHEN 0 THEN '未讀' WHEN 1 THEN '已讀' END AS ReadStatus,
    n.SentTime,
    n.IsReadTime
FROM Notifications n
INNER JOIN Persons p1 ON n.GetId = p1.PersonId
INNER JOIN Users receiver ON p1.UserId = receiver.UserId
INNER JOIN Persons p2 ON n.SendId = p2.PersonId
INNER JOIN Users sender ON p2.UserId = sender.UserId;
```

## 📋 匯入注意事項

### 雙重外鍵依賴檢查
```sql
-- 確認接收者和發送者的 PersonId 都存在
SELECT 'Receiver Check' AS CheckType, COUNT(*) AS Count 
FROM Persons WHERE PersonId IN ('要匯入的GetId列表')
UNION ALL
SELECT 'Sender Check', COUNT(*) 
FROM Persons WHERE PersonId IN ('要匯入的SendId列表');
```

### 必要欄位檢查
- `GetId`: 必須是 Persons 表中已存在的 PersonId
- `SendId`: 必須是 Persons 表中已存在的 PersonId
- `Type`: 0=文章留言，1=用戶私信
- `IsRead`: 0=未讀，1=已讀

### 資料完整性規則
- 用戶不能給自己發送通知（GetId != SendId）
- 已讀通知必須有 IsReadTime
- 未讀通知的 IsReadTime 應為 NULL
- `SentTime`: 使用 GETDATE()

### 業務邏輯考量
- 通知產生時機應與實際操作關聯
- 通知閱讀狀態需要更新機制
- 可能需要批量標記已讀功能

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：用戶給自己發送通知
INSERT INTO Notifications (GetId, SendId, Type) 
VALUES ('98765432-1234-1234-1234-123456789abc', 
        '98765432-1234-1234-1234-123456789abc', 0);
-- Business Logic Error: 用戶不能給自己發送通知
```

### ❌ 邏輯不一致
```sql
-- 錯誤：已讀狀態但沒有閱讀時間
INSERT INTO Notifications (GetId, SendId, Type, IsRead, SentTime, IsReadTime) 
VALUES ('98765432-1234-1234-1234-123456789abc', 
        '98765432-1234-1234-1234-123456789def', 0, 1, GETDATE(), NULL);
-- Logic Error: 已讀通知應該有閱讀時間
```

### ✅ 正確範例
```sql
-- 正確：建立文章留言通知
DECLARE @ReceiverId GUID = (
    SELECT PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'alice_chen'
);
DECLARE @SenderId GUID = (
    SELECT PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'bob_wang'
);

-- 確保不是自我通知
IF @ReceiverId != @SenderId
BEGIN
    INSERT INTO Notifications (GetId, SendId, Type, IsRead, SentTime, IsReadTime)
    VALUES (@ReceiverId, @SenderId, 0, 0, GETDATE(), NULL);
END
```

### 匯入順序建議
1. 先確保 Persons 表有足夠的資料
2. 查詢 Persons 表取得有效的 PersonId
3. 確認不會建立自我通知
4. 根據業務場景選擇適當的 Type
5. 設定合理的閱讀狀態和時間
6. 使用 GETDATE() 設定 SentTime

### 通知狀態管理建議
```sql
-- 標記通知為已讀
UPDATE Notifications 
SET IsRead = 1, IsReadTime = GETDATE()
WHERE NotifyId = @NotificationId 
  AND IsRead = 0;

-- 批量標記用戶所有通知為已讀
UPDATE Notifications 
SET IsRead = 1, IsReadTime = GETDATE()
WHERE GetId = @UserId 
  AND IsRead = 0;

-- 檢查未讀通知數量
SELECT COUNT(*) AS UnreadCount
FROM Notifications 
WHERE GetId = @UserId 
  AND IsRead = 0;
```

### 清理舊通知建議
```sql
-- 刪除 30 天前的已讀通知
DELETE FROM Notifications 
WHERE IsRead = 1 
  AND IsReadTime < DATEADD(DAY, -30, GETDATE());

-- 檢查通知數據完整性
SELECT 
    COUNT(*) AS InconsistentNotifications
FROM Notifications 
WHERE (IsRead = 1 AND IsReadTime IS NULL) 
   OR (IsRead = 0 AND IsReadTime IS NOT NULL);
```