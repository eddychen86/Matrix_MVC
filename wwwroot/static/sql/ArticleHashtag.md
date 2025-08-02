# ArticleHashtag 表假資料匯入說明

## ⚠️ 重要提醒
**無獨立 PK：** ArticleHashtag 表使用 `ArticleId` 和 `TagId` 作為複合主鍵，**不需要自動生成的 ID！**

**雙重外鍵依賴：** 
- `ArticleId` 欄位必須參照已存在的 Articles.ArticleId，不可自己亂寫！
- `TagId` 欄位必須參照已存在的 Hashtags.TagId，不可自己亂寫！

## 表格結構說明
ArticleHashtag 表是文章與標籤之間的多對多關聯表，一篇文章可以有多個標籤，一個標籤可以對應多篇文章。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 備註 |
|-------|---------|-----|---------|------|
| ArticleId | Guid | 文章Id (複合PK) | 是 | FK 到 Articles.ArticleId |
| TagId | Guid | 標籤Id (複合PK) | 是 | FK 到 Hashtags.TagId |

## 🔧 假資料匯入範例

### 匯入前準備：確認相關表資料
```sql
-- 1. 查詢 Articles 表取得有效的 ArticleId
SELECT ArticleId, SUBSTRING(Content, 1, 40) AS ContentPreview, AuthorId FROM Articles;

-- 2. 查詢 Hashtags 表取得有效的 TagId
SELECT TagId, Content FROM Hashtags WHERE Status = 0;
```

#### Articles 表查詢結果：
ArticleId                            | ContentPreview                       | AuthorId
-------------------------------------|--------------------------------------|--------------------------------------
11111111-1111-1111-1111-111111111111 | Web3 技術正在快速發展，DeFi 協議的創新... | 98765432-1234-1234-1234-123456789abc
22222222-2222-2222-2222-222222222222 | 剛完成了一個智能合約專案，使用了最新的... | 98765432-1234-1234-1234-123456789def
33333333-3333-3333-3333-333333333333 | 分享一篇關於 Layer 2 解決方案的深度... | 98765432-1234-1234-1234-123456789abc

#### Hashtags 表查詢結果：
TagId                                | Content
-------------------------------------|----------
wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww | Web3
xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx | DeFi
yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy | Blockchain
zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz | Solidity
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | Layer2
bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | Polygon
cccccccc-cccc-cccc-cccc-cccccccccccc | Arbitrum

### 匯入前資料庫狀態
```sql
SELECT * FROM ArticleHashtags;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Articles 和 Hashtags 表查詢到的真實 ID
-- ArticleId 對應 Articles.ArticleId，TagId 對應 Hashtags.TagId
INSERT INTO ArticleHashtags (ArticleId, TagId)
VALUES 
    -- Alice 的第一篇文章：Web3 + DeFi + Blockchain
    ('11111111-1111-1111-1111-111111111111', 'wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww'), -- Web3
    ('11111111-1111-1111-1111-111111111111', 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'), -- DeFi
    ('11111111-1111-1111-1111-111111111111', 'yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy'), -- Blockchain
    
    -- Bob 的文章：Solidity + Web3
    ('22222222-2222-2222-2222-222222222222', 'zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz'), -- Solidity
    ('22222222-2222-2222-2222-222222222222', 'wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww'), -- Web3
    
    -- Alice 的第二篇文章：Layer2 + Polygon + Arbitrum + Web3
    ('33333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'), -- Layer2
    ('33333333-3333-3333-3333-333333333333', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'), -- Polygon
    ('33333333-3333-3333-3333-333333333333', 'cccccccc-cccc-cccc-cccc-cccccccccccc'), -- Arbitrum
    ('33333333-3333-3333-3333-333333333333', 'wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww'); -- Web3
```

### 匯入後資料庫狀態
```sql
SELECT ArticleId, TagId FROM ArticleHashtags ORDER BY ArticleId, TagId;
```
ArticleId                            | TagId
-------------------------------------|--------------------------------------
11111111-1111-1111-1111-111111111111 | wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww
11111111-1111-1111-1111-111111111111 | xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
11111111-1111-1111-1111-111111111111 | yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy
22222222-2222-2222-2222-222222222222 | wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww
22222222-2222-2222-2222-222222222222 | zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz  
33333333-3333-3333-3333-333333333333 | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
33333333-3333-3333-3333-333333333333 | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb
33333333-3333-3333-3333-333333333333 | cccccccc-cccc-cccc-cccc-cccccccccccc
33333333-3333-3333-3333-333333333333 | wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww

### 驗證匯入結果
```sql
-- 檢查文章標籤關聯詳情
SELECT 
    SUBSTRING(a.Content, 1, 30) AS ArticlePreview,
    STRING_AGG(h.Content, ', ') AS Tags
FROM ArticleHashtags ah
INNER JOIN Articles a ON ah.ArticleId = a.ArticleId
INNER JOIN Hashtags h ON ah.TagId = h.TagId
GROUP BY a.ArticleId, a.Content
ORDER BY a.ArticleId;
```
```
ArticlePreview                     | Tags
-----------------------------------|-------------------------
Web3 技術正在快速發展，DeFi 協議的創新... | Web3, DeFi, Blockchain
剛完成了一個智能合約專案，使用了最新的... | Web3, Solidity
分享一篇關於 Layer 2 解決方案的深度... | Layer2, Polygon, Arbitrum, Web3
```

### 標籤使用統計
```sql
-- 檢查標籤使用頻率
SELECT 
    h.Content AS TagName,
    COUNT(*) AS UsageCount
FROM ArticleHashtags ah
INNER JOIN Hashtags h ON ah.TagId = h.TagId
GROUP BY h.TagId, h.Content
ORDER BY COUNT(*) DESC;
```
TagName    | UsageCount
-----------|------------
Web3       | 3
Layer2     | 1
Polygon    | 1
Arbitrum   | 1
DeFi       | 1
Blockchain | 1
Solidity   | 1

## 📋 匯入注意事項

### 雙重外鍵依賴檢查
```sql
-- 確認 ArticleId 存在於 Articles 表
SELECT COUNT(*) FROM Articles WHERE ArticleId IN ('要匯入的ArticleId列表');

-- 確認 TagId 存在於 Hashtags 表  
SELECT COUNT(*) FROM Hashtags WHERE TagId IN ('要匯入的TagId列表') AND Status = 0;
```

### 複合主鍵限制
- 同一篇文章不能重複關聯同一個標籤
- (ArticleId, TagId) 組合必須唯一

### 資料完整性規則
- 只能關聯狀態為正常的標籤（Status = 0）
- 文章狀態為正常的才建議添加標籤
- 建議每篇文章關聯 3-8 個標籤

### 業務邏輯考量
- 標籤應該與文章內容相關
- 優先使用熱門和通用標籤
- 避免過度標記

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：重複關聯同一標籤
INSERT INTO ArticleHashtags (ArticleId, TagId) 
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww'),
    ('11111111-1111-1111-1111-111111111111', 'wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww');
-- Error: PRIMARY KEY constraint failed (複合主鍵重複)
```

### ❌ 外鍵不存在
```sql
-- 錯誤：使用不存在的 ArticleId 或 TagId
INSERT INTO ArticleHashtags (ArticleId, TagId) 
VALUES ('99999999-9999-9999-9999-999999999999', 
        '88888888-8888-8888-8888-888888888888');
-- Error: FOREIGN KEY constraint failed
```

### ✅ 正確範例
```sql
-- 正確：先檢查然後插入
DECLARE @ArticleId GUID = (SELECT TOP 1 ArticleId FROM Articles WHERE Status = 0);
DECLARE @TagId GUID = (SELECT TOP 1 TagId FROM Hashtags WHERE Content = 'Web3' AND Status = 0);

-- 檢查是否已存在關聯
IF NOT EXISTS (
    SELECT 1 FROM ArticleHashtags 
    WHERE ArticleId = @ArticleId AND TagId = @TagId
)
BEGIN
    INSERT INTO ArticleHashtags (ArticleId, TagId) 
    VALUES (@ArticleId, @TagId);
END
```

### 匯入順序建議
1. 先確保 Articles 和 Hashtags 表有資料
2. 查詢兩個表取得有效的 ID
3. 確認文章和標籤都是正常狀態
4. 檢查是否已存在關聯避免重複
5. 批量插入關聯資料

### 批量關聯建議
```sql
-- 為文章批量添加相關標籤
WITH ArticleTags AS (
    SELECT a.ArticleId, h.TagId
    FROM Articles a, Hashtags h
    WHERE a.Status = 0 
      AND h.Status = 0
      AND (
          (a.Content LIKE '%Web3%' AND h.Content = 'Web3') OR
          (a.Content LIKE '%DeFi%' AND h.Content = 'DeFi') OR
          (a.Content LIKE '%區塊鏈%' AND h.Content = 'Blockchain') OR
          (a.Content LIKE '%智能合約%' AND h.Content = 'Solidity')
      )
)
INSERT INTO ArticleHashtags (ArticleId, TagId)
SELECT at.ArticleId, at.TagId
FROM ArticleTags at
WHERE NOT EXISTS (
    SELECT 1 FROM ArticleHashtags ah 
    WHERE ah.ArticleId = at.ArticleId AND ah.TagId = at.TagId
);
```

### 關聯維護建議
```sql
-- 清理無效關聯（文章或標籤已被刪除/停用）
DELETE FROM ArticleHashtags 
WHERE ArticleId IN (
    SELECT ArticleId FROM Articles WHERE Status != 0
) OR TagId IN (
    SELECT TagId FROM Hashtags WHERE Status != 0
);

-- 檢查孤立關聯
SELECT 
    'Articles' AS TableName,
    COUNT(*) AS OrphanCount
FROM ArticleHashtags ah
LEFT JOIN Articles a ON ah.ArticleId = a.ArticleId
WHERE a.ArticleId IS NULL
UNION ALL
SELECT 
    'Hashtags',
    COUNT(*)
FROM ArticleHashtags ah
LEFT JOIN Hashtags h ON ah.TagId = h.TagId
WHERE h.TagId IS NULL;
```

### 標籤關聯統計
```sql
-- 查看文章標籤分布
SELECT 
    TagCount,
    COUNT(*) AS ArticleCount,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Articles), 2) AS Percentage
FROM (
    SELECT 
        a.ArticleId,
        COUNT(ah.TagId) AS TagCount
    FROM Articles a
    LEFT JOIN ArticleHashtags ah ON a.ArticleId = ah.ArticleId
    GROUP BY a.ArticleId
) AS ArticleTagCounts
GROUP BY TagCount
ORDER BY TagCount;
```