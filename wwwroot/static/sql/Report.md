# Report 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Report 表的主鍵 `ReportId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**外鍵依賴：** 
- `ReporterId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！
- `ResolverId` 欄位如果不為 NULL，必須參照已存在的 Persons.PersonId！
- `TargetId` 是靈活外鍵，可能指向 Articles.ArticleId 或 Persons.PersonId

## 表格結構說明
Report 表記錄用戶對文章或其他用戶的舉報，包含舉報原因、處理狀態等資訊。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| ReportId | Guid | 舉報唯一識別碼 (PK) | 是 | **自動生成** |
| ReporterId | Guid | 舉報者PersonId (FK) | 是 | - |
| TargetId | Guid | 被舉報對象Id | 是 | - |
| Type | int | 舉報類型 | 是 | - |
| Reason | string(500) | 舉報原因 | 是 | - |
| Status | int | 處理狀態 (0=待處理) | 是 | 0 |
| ResolverId | Guid | 處理者PersonId (FK) | 否 | NULL |
| ProcessTime | datetime | 處理時間 | 否 | NULL |

### 舉報類型說明
- `Type = 0`: 舉報文章
- `Type = 1`: 舉報用戶
- 其他類型可依業務需求擴展

### 處理狀態說明
- `Status = 0`: 待處理
- `Status = 1`: 已處理 - 成立
- `Status = 2`: 已處理 - 不成立
- `Status = 3`: 已處理 - 需要更多資訊

## 🔧 假資料匯入範例

### 匯入前準備：確認相關表資料
```sql
-- 1. 查詢 Persons 表取得有效的 PersonId
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId;

-- 2. 查詢 Articles 表取得有效的 ArticleId  
SELECT ArticleId, SUBSTRING(Content, 1, 30) AS ContentPreview FROM Articles;

-- 3. 查詢管理員用戶（Role = 1）
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId
WHERE Users.Role = 1;
```

#### 相關資料查詢結果：
-- Persons:
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789abc | Alice Chen
98765432-1234-1234-1234-123456789def | Bob Wang  
98765432-1234-1234-1234-123456789ghi | Matrix Admin

-- Articles:
ArticleId                            | ContentPreview
-------------------------------------|-----------------------------
11111111-1111-1111-1111-111111111111 | Web3 技術正在快速發展，DeFi...
22222222-2222-2222-2222-222222222222 | 剛完成了一個智能合約專案...

-- Admins:
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789ghi | Matrix Admin

### 匯入前資料庫狀態
```sql
SELECT * FROM Reports;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從相關表查詢到的真實 ID
INSERT INTO Reports (ReporterId, TargetId, Type, Reason, Status, ResolverId, ProcessTime)
VALUES 
    -- Alice 舉報某篇文章內容不當 (Type = 0: 舉報文章，Status = 0: 待處理)
    ('98765432-1234-1234-1234-123456789abc', 
     '22222222-2222-2222-2222-222222222222',  -- Bob 的文章
     0, '文章內容與平台主題不符，包含過多非技術相關內容', 0, NULL, NULL),
     
    -- Bob 舉報 Alice 用戶行為不當 (Type = 1: 舉報用戶，Status = 1: 已處理-成立)
    ('98765432-1234-1234-1234-123456789def', 
     '98765432-1234-1234-1234-123456789abc',  -- Alice
     1, '用戶在回覆中使用不當言論，影響社群氛圍', 1, 
     '98765432-1234-1234-1234-123456789ghi',  -- Admin 處理
     DATEADD(HOUR, -2, GETDATE())),
     
    -- Alice 舉報另一篇文章 (Type = 0: 舉報文章，Status = 2: 已處理-不成立)
    ('98765432-1234-1234-1234-123456789abc', 
     '11111111-1111-1111-1111-111111111111',  -- Alice 自己的文章（測試用）
     0, '測試舉報功能', 2, 
     '98765432-1234-1234-1234-123456789ghi',  -- Admin 處理
     DATEADD(HOUR, -1, GETDATE()));
```

### 匯入後資料庫狀態
```sql
SELECT ReportId, ReporterId, TargetId, Type, SUBSTRING(Reason, 1, 30) AS ReasonPreview, 
       Status, ResolverId, ProcessTime 
FROM Reports
ORDER BY ProcessTime DESC NULLS LAST;
```
ReportId                             | ReporterId                           | TargetId                             | Type | ReasonPreview           | Status | ResolverId                           | ProcessTime
-------------------------------------|--------------------------------------|--------------------------------------|------|------------------------|--------|--------------------------------------|---------------------
qqqqqqqq-qqqq-qqqq-qqqq-qqqqqqqqqqqq | 98765432-1234-1234-1234-123456789abc | 22222222-2222-2222-2222-222222222222 | 0    | 文章內容與平台主題不符...   | 0      | NULL                                 | NULL
rrrrrrrr-rrrr-rrrr-rrrr-rrrrrrrrrrrr | 98765432-1234-1234-1234-123456789def | 98765432-1234-1234-1234-123456789abc | 1    | 用戶在回覆中使用不當言論... | 1      | 98765432-1234-1234-1234-123456789ghi | 2024-01-01 13:00:00
ssssssss-ssss-ssss-ssss-ssssssssssss | 98765432-1234-1234-1234-123456789abc | 11111111-1111-1111-1111-111111111111 | 0    | 測試舉報功能            | 2      | 98765432-1234-1234-1234-123456789ghi | 2024-01-01 14:00:00

### 驗證匯入結果
```sql
-- 檢查舉報類型分布
SELECT Type, 
       CASE Type 
           WHEN 0 THEN '舉報文章'
           WHEN 1 THEN '舉報用戶'
           ELSE '其他'
       END AS TypeName,
       COUNT(*) AS Count
FROM Reports 
GROUP BY Type;

-- 檢查處理狀態分布
SELECT Status,
       CASE Status 
           WHEN 0 THEN '待處理'
           WHEN 1 THEN '已處理-成立'
           WHEN 2 THEN '已處理-不成立'
           WHEN 3 THEN '需要更多資訊'
       END AS StatusName,
       COUNT(*) AS Count
FROM Reports 
GROUP BY Status;

-- 檢查舉報詳情
SELECT 
    reporter.DisplayName AS Reporter,
    CASE r.Type 
        WHEN 0 THEN '文章: ' + SUBSTRING(a.Content, 1, 20) + '...'
        WHEN 1 THEN '用戶: ' + target.DisplayName
        ELSE '其他'
    END AS Target,
    SUBSTRING(r.Reason, 1, 40) AS ReasonPreview,
    CASE r.Status 
        WHEN 0 THEN '待處理'
        WHEN 1 THEN '已處理-成立'
        WHEN 2 THEN '已處理-不成立'
    END AS Status,
    resolver.DisplayName AS Resolver
FROM Reports r
INNER JOIN Persons rp ON r.ReporterId = rp.PersonId
INNER JOIN Users reporter ON rp.UserId = reporter.UserId
LEFT JOIN Articles a ON r.TargetId = a.ArticleId AND r.Type = 0
LEFT JOIN Persons tp ON r.TargetId = tp.PersonId AND r.Type = 1
LEFT JOIN Users target ON tp.UserId = target.UserId
LEFT JOIN Persons resp ON r.ResolverId = resp.PersonId
LEFT JOIN Users resolver ON resp.UserId = resolver.UserId;
```

## 📋 匯入注意事項

### 外鍵依賴檢查
```sql
-- 確認舉報者存在
SELECT COUNT(*) FROM Persons WHERE PersonId IN ('要匯入的ReporterId列表');

-- 確認處理者存在（如果不為 NULL）
SELECT COUNT(*) FROM Persons WHERE PersonId IN ('要匯入的ResolverId列表');

-- 根據類型確認目標對象存在
-- Type = 0: 確認 ArticleId 存在
SELECT COUNT(*) FROM Articles WHERE ArticleId IN ('要匯入的TargetId列表');
-- Type = 1: 確認 PersonId 存在  
SELECT COUNT(*) FROM Persons WHERE PersonId IN ('要匯入的TargetId列表');
```

### 必要欄位檢查
- `ReporterId`: 必須是 Persons 表中已存在的 PersonId
- `TargetId`: 根據 Type 對應 Articles.ArticleId 或 Persons.PersonId
- `Reason`: 不可為空，最大長度 500 字元
- `Type`: 0=舉報文章，1=舉報用戶

### 資料完整性規則
- 用戶不能舉報自己（ReporterId != TargetId when Type = 1）
- 已處理的舉報必須有 ResolverId 和 ProcessTime
- 待處理的舉報 ResolverId 和 ProcessTime 應為 NULL
- 處理者應該是管理員（Role = 1）

### 業務邏輯考量
- 舉報原因應該具體明確
- 處理結果需要通知相關用戶
- 可能需要根據舉報結果執行相應操作

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：用戶舉報自己
INSERT INTO Reports (ReporterId, TargetId, Type, Reason) 
VALUES ('98765432-1234-1234-1234-123456789abc', 
        '98765432-1234-1234-1234-123456789abc', 1, '測試');
-- Business Logic Error: 用戶不能舉報自己
```

### ❌ 邏輯不一致
```sql
-- 錯誤：已處理狀態但沒有處理者和時間
INSERT INTO Reports (ReporterId, TargetId, Type, Reason, Status, ResolverId, ProcessTime) 
VALUES ('98765432-1234-1234-1234-123456789abc', 
        '11111111-1111-1111-1111-111111111111', 0, '測試', 1, NULL, NULL);
-- Logic Error: 已處理舉報應該有處理者和處理時間
```

### ✅ 正確範例
```sql
-- 正確：建立文章舉報
DECLARE @ReporterId GUID = (
    SELECT PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'alice_chen'
);
DECLARE @ArticleId GUID = (SELECT TOP 1 ArticleId FROM Articles);

-- 確保不是舉報自己的文章
IF NOT EXISTS (
    SELECT 1 FROM Articles a 
    INNER JOIN Persons p ON a.AuthorId = p.PersonId 
    WHERE a.ArticleId = @ArticleId AND p.PersonId = @ReporterId
)
BEGIN
    INSERT INTO Reports (ReporterId, TargetId, Type, Reason, Status, ResolverId, ProcessTime)
    VALUES (@ReporterId, @ArticleId, 0, '文章內容不符合平台規範', 0, NULL, NULL);
END
```

### 匯入順序建議
1. 先確保 Persons、Articles 表有資料
2. 確認管理員用戶存在（用於處理者）
3. 查詢相關表取得有效的 ID
4. 確認舉報邏輯合理（不能自我舉報）
5. 根據處理狀態設定 ResolverId 和 ProcessTime
6. 舉報原因應該具體且有意義

### 舉報處理建議
```sql
-- 處理舉報（管理員操作）
UPDATE Reports 
SET Status = 1,  -- 成立
    ResolverId = @AdminPersonId,
    ProcessTime = GETDATE()
WHERE ReportId = @ReportId 
  AND Status = 0;

-- 查詢待處理舉報
SELECT 
    r.ReportId,
    reporter.DisplayName AS Reporter,
    CASE r.Type 
        WHEN 0 THEN '文章舉報'
        WHEN 1 THEN '用戶舉報'
    END AS ReportType,
    r.Reason,
    r.ProcessTime
FROM Reports r
INNER JOIN Persons rp ON r.ReporterId = rp.PersonId
INNER JOIN Users reporter ON rp.UserId = reporter.UserId
WHERE r.Status = 0
ORDER BY r.ProcessTime DESC;
```

### 數據完整性檢查
```sql
-- 檢查邏輯不一致的舉報
SELECT 
    COUNT(*) AS InconsistentReports
FROM Reports 
WHERE (Status != 0 AND (ResolverId IS NULL OR ProcessTime IS NULL))
   OR (Status = 0 AND (ResolverId IS NOT NULL OR ProcessTime IS NOT NULL));

-- 檢查自我舉報
SELECT COUNT(*) AS SelfReports
FROM Reports r
INNER JOIN Articles a ON r.TargetId = a.ArticleId
WHERE r.Type = 0 AND r.ReporterId = a.AuthorId;
```