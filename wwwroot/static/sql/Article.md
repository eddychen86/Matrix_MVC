# Article 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Article 表的主鍵 `ArticleId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**外鍵依賴：** `AuthorId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！

## 表格結構說明
Article 表存儲系統中的文章內容，每篇文章都有一個作者（Person）。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| ArticleId | Guid | 文章唯一識別碼 (PK) | 是 | **自動生成** |
| AuthorId | Guid | 作者PersonId (FK) | 是 | - |
| Content | string(4000) | 文章內容 | 是 | - |
| IsPublic | int | 公開狀態 (0=公開) | 是 | 0 |
| Status | int | 文章狀態 (0=正常) | 是 | 0 |
| CreateTime | datetime | 建立時間 | 是 | GETDATE() |
| PraiseCount | int | 讚數量 | 是 | 0 |
| CollectCount | int | 收藏數量 | 是 | 0 |

## 🔧 假資料匯入範例

### 匯入前準備：確認 Persons 表資料
```sql
-- 必須先查詢 Persons 表取得有效的 PersonId
SELECT PersonId, DisplayName FROM Persons 
INNER JOIN Users ON Persons.UserId = Users.UserId;
```
PersonId                             | DisplayName
-------------------------------------|-------------
98765432-1234-1234-1234-123456789abc | Alice Chen
98765432-1234-1234-1234-123456789def | Bob Wang
98765432-1234-1234-1234-123456789ghi | Matrix Admin

### 匯入前資料庫狀態
```sql
SELECT * FROM Articles;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Persons 表查詢到的真實 PersonId 作為 AuthorId
INSERT INTO Articles (AuthorId, Content, IsPublic, Status, CreateTime, PraiseCount, CollectCount)
VALUES 
    ('98765432-1234-1234-1234-123456789abc', 
     'Web3 技術正在快速發展，DeFi 協議的創新為金融領域帶來了新的可能性。我們需要關注安全性和用戶體驗的平衡。', 
     0, 0, GETDATE(), 0, 0),
     
    ('98765432-1234-1234-1234-123456789def', 
     '剛完成了一個智能合約專案，使用了最新的 Solidity 0.8 版本。在開發過程中發現了一些有趣的 gas 優化技巧。', 
     0, 0, GETDATE(), 0, 0),
     
    ('98765432-1234-1234-1234-123456789abc', 
     '分享一篇關於 Layer 2 解決方案的深度分析，Polygon 和 Arbitrum 的比較研究。', 
     0, 0, GETDATE(), 0, 0);
```

### 匯入後資料庫狀態
```sql
SELECT ArticleId, AuthorId, SUBSTRING(Content, 1, 50) AS ContentPreview, 
       IsPublic, Status, CreateTime, PraiseCount, CollectCount 
FROM Articles;
```
ArticleId                            | AuthorId                             | ContentPreview                      | IsPublic | Status | CreateTime          | PraiseCount | CollectCount
-------------------------------------|--------------------------------------|-------------------------------------|----------|--------|---------------------|------------|-------------
11111111-1111-1111-1111-111111111111 | 98765432-1234-1234-1234-123456789abc | Web3 技術正在快速發展，DeFi 協議的創新... | 0        | 0      | 2024-01-01 10:30:00 | 0          | 0
22222222-2222-2222-2222-222222222222 | 98765432-1234-1234-1234-123456789def | 剛完成了一個智能合約專案，使用了最新的...  | 0        | 0      | 2024-01-01 10:31:00 | 0          | 0
33333333-3333-3333-3333-333333333333 | 98765432-1234-1234-1234-123456789abc | 分享一篇關於 Layer 2 解決方案的深度... | 0        | 0      | 2024-01-01 10:32:00 | 0          | 0

## 📋 匯入注意事項

### 外鍵依賴檢查
```sql
-- 匯入前必須先確認 Persons 表中存在對應的 PersonId
SELECT PersonId FROM Persons WHERE PersonId IN ('要匯入的AuthorId列表');
```

### 必要欄位檢查
- `AuthorId`: 必須是 Persons 表中已存在的 PersonId
- `Content`: 不可為空，最大長度 4000 字元

### 資料完整性規則
- `IsPublic`: 0=公開，1=私人
- `Status`: 0=正常，1=已刪除，2=審核中
- `PraiseCount`, `CollectCount`: 初始值通常為 0
- `CreateTime`: 建議使用 GETDATE()

### 關聯影響
新增 Article 後，可能會影響：
- Reply 表（文章回覆）
- PraiseCollect 表（讚與收藏）
- ArticleHashtag 表（文章標籤）
- ArticleAttachment 表（文章附件）

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：使用不存在的 AuthorId
INSERT INTO Articles (AuthorId, Content) 
VALUES ('99999999-9999-9999-9999-999999999999', 'Test Article');
-- Error: FOREIGN KEY constraint failed
```

### ✅ 正確範例
```sql
-- 正確：先查詢再使用
DECLARE @AuthorId GUID = (
    SELECT TOP 1 PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'alice_chen'
);
INSERT INTO Articles (AuthorId, Content, CreateTime) 
VALUES (@AuthorId, 'Web3 技術分享文章', GETDATE());
```

### 匯入順序建議
1. 先確保 Users 和 Persons 表有資料
2. 查詢 Persons 表取得有效的 PersonId
3. 使用查詢到的 PersonId 作為 AuthorId
4. 使用 GETDATE() 設定 CreateTime
5. 初始 PraiseCount 和 CollectCount 設為 0

### 內容建議
- 文章內容應符合平台的 Web3 和深度技術討論主題
- 避免娛樂性內容，專注於技術分享和討論
- 內容長度控制在 4000 字元以內