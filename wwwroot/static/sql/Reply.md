# Reply 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Reply 表的主鍵 `ReplyId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**雙重外鍵依賴：** 
- `UserId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！
- `ArticleId` 欄位必須參照已存在的 Articles.ArticleId，不可自己亂寫！

## 表格結構說明
Reply 表存儲用戶對文章的回覆，建立 Person 和 Article 之間的多對多關聯。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| ReplyId | Guid | 回覆唯一識別碼 (PK) | 是 | **自動生成** |
| UserId | Guid | 回覆者PersonId (FK) | 是 | - |
| ArticleId | Guid | 被回覆文章Id (FK) | 是 | - |
| Content | string(1000) | 回覆內容 | 是 | - |
| ReplyTime | datetime | 回覆時間 | 是 | GETDATE() |

## 🔧 假資料匯入範例

### 匯入前準備：確認相關表資料
```sql
-- 1. 查詢 Persons 表取得有效的 PersonId
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId;

-- 2. 查詢 Articles 表取得有效的 ArticleId
SELECT ArticleId, SUBSTRING(Content, 1, 30) AS ContentPreview, 
       AuthorId FROM Articles;
```

#### Persons 表查詢結果：
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789abc | Alice Chen
98765432-1234-1234-1234-123456789def | Bob Wang  
98765432-1234-1234-1234-123456789ghi | Matrix Admin

#### Articles 表查詢結果：
ArticleId                            | ContentPreview              | AuthorId
-------------------------------------|----------------------------|--------------------------------------
11111111-1111-1111-1111-111111111111 | Web3 技術正在快速發展，DeFi... | 98765432-1234-1234-1234-123456789abc
22222222-2222-2222-2222-222222222222 | 剛完成了一個智能合約專案...     | 98765432-1234-1234-1234-123456789def
33333333-3333-3333-3333-333333333333 | 分享一篇關於 Layer 2 解決... | 98765432-1234-1234-1234-123456789abc

### 匯入前資料庫狀態
```sql
SELECT * FROM Replies;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Persons 和 Articles 表查詢到的真實 ID
INSERT INTO Replies (UserId, ArticleId, Content, ReplyTime)
VALUES 
    -- Bob 回覆 Alice 的第一篇文章
    ('98765432-1234-1234-1234-123456789def', 
     '11111111-1111-1111-1111-111111111111',
     '很棒的分析！DeFi 的安全性確實是當前最重要的挑戰之一。特別是智能合約審計這塊。', 
     GETDATE()),
     
    -- Alice 回覆 Bob 的文章
    ('98765432-1234-1234-1234-123456789abc', 
     '22222222-2222-2222-2222-222222222222',
     '感謝分享！能否詳細說明一下你提到的 gas 優化技巧？我最近也在研究這個領域。', 
     GETDATE()),
     
    -- Admin 回覆 Alice 的第二篇文章
    ('98765432-1234-1234-1234-123456789ghi', 
     '33333333-3333-3333-3333-333333333333',
     '非常詳細的技術分析，建議可以加上實際的效能比較數據會更有說服力。', 
     GETDATE());
```

### 匯入後資料庫狀態
```sql
SELECT ReplyId, UserId, ArticleId, SUBSTRING(Content, 1, 40) AS ContentPreview, ReplyTime 
FROM Replies;
```
ReplyId                              | UserId                               | ArticleId                            | ContentPreview                     | ReplyTime
-------------------------------------|--------------------------------------|--------------------------------------|-----------------------------------|---------------------
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | 98765432-1234-1234-1234-123456789def | 11111111-1111-1111-1111-111111111111 | 很棒的分析！DeFi 的安全性確實是當前... | 2024-01-01 11:00:00
bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | 98765432-1234-1234-1234-123456789abc | 22222222-2222-2222-2222-222222222222 | 感謝分享！能否詳細說明一下你提到的... | 2024-01-01 11:01:00
cccccccc-cccc-cccc-cccc-cccccccccccc | 98765432-1234-1234-1234-123456789ghi | 33333333-3333-3333-3333-333333333333 | 非常詳細的技術分析，建議可以加上... | 2024-01-01 11:02:00

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
- `UserId`: 必須是 Persons 表中已存在的 PersonId
- `ArticleId`: 必須是 Articles 表中已存在的 ArticleId  
- `Content`: 不可為空，最大長度 1000 字元

### 資料完整性規則
- 一個用戶可以對同一篇文章回覆多次
- 回覆內容應該相關且有意義
- `ReplyTime`: 建議使用 GETDATE()

### 業務邏輯考量
- 用戶不能回覆已刪除的文章（Status != 0）
- 私人文章的回覆權限需要檢查
- 被封禁用戶不能發表回覆

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：使用不存在的 UserId 或 ArticleId
INSERT INTO Replies (UserId, ArticleId, Content) 
VALUES ('99999999-9999-9999-9999-999999999999', 
        '88888888-8888-8888-8888-888888888888',
        'Test Reply');
-- Error: FOREIGN KEY constraint failed
```

### ✅ 正確範例
```sql
-- 正確：先查詢確認 ID 存在
DECLARE @UserId GUID = (SELECT TOP 1 PersonId FROM Persons);
DECLARE @ArticleId GUID = (SELECT TOP 1 ArticleId FROM Articles);

INSERT INTO Replies (UserId, ArticleId, Content, ReplyTime)
VALUES (@UserId, @ArticleId, '這是一個有意義的回覆內容', GETDATE());
```

### 匯入順序建議
1. 先確保 Users、Persons 和 Articles 表有資料
2. 查詢 Persons 表取得有效的 PersonId
3. 查詢 Articles 表取得有效的 ArticleId
4. 確認文章狀態為正常（Status = 0）
5. 使用 GETDATE() 設定 ReplyTime

### 回覆內容建議
- 回覆應該與文章主題相關
- 內容應該有建設性和討論價值
- 符合平台的 Web3 和技術討論氛圍
- 長度控制在 1000 字元以內