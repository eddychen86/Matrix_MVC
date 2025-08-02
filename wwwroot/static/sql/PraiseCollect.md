# PraiseCollect 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** PraiseCollect 表的主鍵 `EventId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**雙重外鍵依賴：** 
- `UserId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！
- `ArticleId` 欄位必須參照已存在的 Articles.ArticleId，不可自己亂寫！

## 表格結構說明
PraiseCollect 表記錄用戶對文章的讚和收藏操作，建立 Person 和 Article 之間的互動關係。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| EventId | Guid | 事件唯一識別碼 (PK) | 是 | **自動生成** |
| Type | int | 操作類型 (0=讚, 1=收藏) | 是 | - |
| UserId | Guid | 操作用戶PersonId (FK) | 是 | - |
| ArticleId | Guid | 目標文章Id (FK) | 是 | - |
| CreateTime | datetime | 操作時間 | 是 | GETDATE() |

## 🔧 假資料匯入範例

### 匯入前準備：確認相關表資料
```sql
-- 1. 查詢 Persons 表取得有效的 PersonId
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId;

-- 2. 查詢 Articles 表取得有效的 ArticleId
SELECT ArticleId, SUBSTRING(Content, 1, 30) AS ContentPreview FROM Articles;
```

#### Persons 表查詢結果：
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789abc | Alice Chen
98765432-1234-1234-1234-123456789def | Bob Wang  
98765432-1234-1234-1234-123456789ghi | Matrix Admin

#### Articles 表查詢結果：
ArticleId                            | ContentPreview
-------------------------------------|-----------------------------
11111111-1111-1111-1111-111111111111 | Web3 技術正在快速發展，DeFi...
22222222-2222-2222-2222-222222222222 | 剛完成了一個智能合約專案...
33333333-3333-3333-3333-333333333333 | 分享一篇關於 Layer 2 解決...

### 匯入前資料庫狀態
```sql
SELECT * FROM PraiseCollects;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Persons 和 Articles 表查詢到的真實 ID
-- UserId 對應 Persons.PersonId，ArticleId 對應 Articles.ArticleId
-- Type: 0(讚) 或 1(收藏)，時間一律使用 GETDATE()
INSERT INTO PraiseCollects (Type, UserId, ArticleId, CreateTime)
VALUES 
    -- Bob 給 Alice 的第一篇文章按讚
    (0, '98765432-1234-1234-1234-123456789def', 
        '11111111-1111-1111-1111-111111111111', 
        GETDATE()),
     
    -- Alice 收藏 Bob 的文章
    (1, '98765432-1234-1234-1234-123456789abc', 
        '22222222-2222-2222-2222-222222222222', 
        GETDATE()),
     
    -- Admin 給 Alice 的第二篇文章按讚
    (0, '98765432-1234-1234-1234-123456789ghi', 
        '33333333-3333-3333-3333-333333333333', 
        GETDATE()),
        
    -- Bob 也收藏 Alice 的第二篇文章
    (1, '98765432-1234-1234-1234-123456789def', 
        '33333333-3333-3333-3333-333333333333', 
        GETDATE());
```

### 匯入後資料庫狀態
```sql
SELECT EventId, Type, UserId, ArticleId, CreateTime 
FROM PraiseCollects
ORDER BY CreateTime;
```
EventId                              | Type | UserId                               | ArticleId                            | CreateTime
-------------------------------------|------|--------------------------------------|--------------------------------------|---------------------
dddddddd-dddd-dddd-dddd-dddddddddddd | 0    | 98765432-1234-1234-1234-123456789def | 11111111-1111-1111-1111-111111111111 | 2024-01-01 12:00:00
eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee | 1    | 98765432-1234-1234-1234-123456789abc | 22222222-2222-2222-2222-222222222222 | 2024-01-01 12:00:01
ffffffff-ffff-ffff-ffff-ffffffffffff | 0    | 98765432-1234-1234-1234-123456789ghi | 33333333-3333-3333-3333-333333333333 | 2024-01-01 12:00:02
gggggggg-gggg-gggg-gggg-gggggggggggg | 1    | 98765432-1234-1234-1234-123456789def | 33333333-3333-3333-3333-333333333333 | 2024-01-01 12:00:03

### 驗證匯入結果
```sql
-- 檢查操作類型分布
SELECT Type, 
       CASE Type WHEN 0 THEN '讚' WHEN 1 THEN '收藏' END AS TypeName,
       COUNT(*) AS Count
FROM PraiseCollects 
GROUP BY Type;
```
Type | TypeName | Count
-----|----------|------
0    | 讚       | 2
1    | 收藏     | 2

## 📋 匯入注意事項

### 雙重外鍵依賴檢查
```sql
-- 匯入前必須確認兩個外鍵都存在
SELECT 'Person Check' AS CheckType, COUNT(*) AS Count 
FROM Persons WHERE PersonId IN ('要匯入的UserId列表')
UNION ALL
SELECT 'Article Check', COUNT(*) 
FROM Articles WHERE ArticleId IN ('要匯入的ArticleId列表');
```

### 必要欄位檢查
- `UserId`: 必須是 Persons 表中已存在的 PersonId（注意：FK 對應到 Persons.PersonId）
- `ArticleId`: 必須是 Articles 表中已存在的 ArticleId
- `Type`: 0=讚，1=收藏

### 資料完整性規則
- 同一用戶對同一文章的同一類型操作應該唯一（業務邏輯）
- 用戶不能對自己的文章進行讚/收藏（業務邏輯）
- `CreateTime`: 一律使用 GETDATE()

### 業務邏輯考量
- 用戶取消讚/收藏時，應刪除對應記錄
- 需要更新 Articles 表的 PraiseCount 和 CollectCount

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：使用不存在的 UserId 或 ArticleId
INSERT INTO PraiseCollects (Type, UserId, ArticleId) 
VALUES (0, '99999999-9999-9999-9999-999999999999', 
           '88888888-8888-8888-8888-888888888888');
-- Error: FOREIGN KEY constraint failed
```

### ❌ 錯誤的外鍵理解
```sql
-- 錯誤：UserId 使用 Users.UserId 而不是 Persons.PersonId
INSERT INTO PraiseCollects (Type, UserId, ArticleId) 
VALUES (0, '12345678-1234-1234-1234-123456789abc', -- 這是 Users.UserId
           '11111111-1111-1111-1111-111111111111');
-- Error: FOREIGN KEY constraint failed
```

### ✅ 正確範例
```sql
-- 正確：UserId 對應 Persons.PersonId，ArticleId 對應 Articles.ArticleId
DECLARE @PersonId GUID = (
    SELECT PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'bob_wang'
);
DECLARE @ArticleId GUID = (SELECT TOP 1 ArticleId FROM Articles);

INSERT INTO PraiseCollects (Type, UserId, ArticleId, CreateTime)
VALUES (0, @PersonId, @ArticleId, GETDATE());
```

### 匯入順序建議
1. 先確保 Users、Persons 和 Articles 表有資料
2. 查詢 Persons 表取得有效的 PersonId（不是 Users.UserId！）
3. 查詢 Articles 表取得有效的 ArticleId
4. 選擇 Type：0(讚) 或 1(收藏)
5. 時間一律使用 GETDATE()

### 資料維護建議
匯入 PraiseCollects 後，建議同步更新 Articles 表的計數：
```sql
-- 更新文章的讚數量
UPDATE Articles 
SET PraiseCount = (
    SELECT COUNT(*) FROM PraiseCollects 
    WHERE ArticleId = Articles.ArticleId AND Type = 0
);

-- 更新文章的收藏數量  
UPDATE Articles 
SET CollectCount = (
    SELECT COUNT(*) FROM PraiseCollects 
    WHERE ArticleId = Articles.ArticleId AND Type = 1
);
```