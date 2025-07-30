# ArticleAttachment 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** ArticleAttachment 表的主鍵 `FileId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**外鍵依賴：** `ArticleId` 欄位必須參照已存在的 Articles.ArticleId，不可自己亂寫！

## 表格結構說明
ArticleAttachment 表存儲文章的附件資訊，包括圖片、文件等多媒體內容的路徑和元資料。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| FileId | Guid | 附件唯一識別碼 (PK) | 是 | **自動生成** |
| ArticleId | Guid | 關聯文章Id (FK) | 是 | - |
| FilePath | string | 附件儲存路徑 | 是 | - |
| Type | string | 附件類型 | 是 | - |
| FileName | string | 原始檔案名稱 | 否 | NULL |
| MimeType | string | MIME 類型 | 否 | NULL |

### 附件類型說明
- `Type = "image"`: 圖片類型附件
- `Type = "file"`: 一般檔案附件
- 其他類型可依業務需求擴展

## 🔧 假資料匯入範例

### 匯入前準備：確認 Articles 表資料
```sql
-- 查詢 Articles 表取得有效的 ArticleId
SELECT ArticleId, SUBSTRING(Content, 1, 40) AS ContentPreview, AuthorId FROM Articles;
```

#### Articles 表查詢結果：
ArticleId                            | ContentPreview                       | AuthorId
-------------------------------------|--------------------------------------|--------------------------------------
11111111-1111-1111-1111-111111111111 | Web3 技術正在快速發展，DeFi 協議的創新... | 98765432-1234-1234-1234-123456789abc
22222222-2222-2222-2222-222222222222 | 剛完成了一個智能合約專案，使用了最新的... | 98765432-1234-1234-1234-123456789def
33333333-3333-3333-3333-333333333333 | 分享一篇關於 Layer 2 解決方案的深度... | 98765432-1234-1234-1234-123456789abc

### 匯入前資料庫狀態
```sql
SELECT * FROM ArticleAttachments;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Articles 表查詢到的真實 ArticleId
INSERT INTO ArticleAttachments (ArticleId, FilePath, Type, FileName, MimeType)
VALUES 
    -- Alice 第一篇文章的圖片附件
    ('11111111-1111-1111-1111-111111111111', 
     '/uploads/articles/2024/01/defi-architecture-diagram.png', 
     'image', 'DeFi架構圖.png', 'image/png'),
     
    -- Alice 第一篇文章的 PDF 文檔
    ('11111111-1111-1111-1111-111111111111', 
     '/uploads/articles/2024/01/defi-whitepaper.pdf', 
     'file', 'DeFi白皮書.pdf', 'application/pdf'),
     
    -- Bob 文章的程式碼截圖
    ('22222222-2222-2222-2222-222222222222', 
     '/uploads/articles/2024/01/solidity-code-example.jpg', 
     'image', 'Solidity程式碼範例.jpg', 'image/jpeg'),
     
    -- Bob 文章的智能合約檔案
    ('22222222-2222-2222-2222-222222222222', 
     '/uploads/articles/2024/01/smart-contract.sol', 
     'file', 'MyContract.sol', 'text/plain'),
     
    -- Alice 第二篇文章的比較圖表
    ('33333333-3333-3333-3333-333333333333', 
     '/uploads/articles/2024/01/layer2-comparison-chart.png', 
     'image', 'Layer2比較圖表.png', 'image/png'),
     
    -- Alice 第二篇文章的技術文檔
    ('33333333-3333-3333-3333-333333333333', 
     '/uploads/articles/2024/01/layer2-technical-analysis.docx', 
     'file', 'Layer2技術分析.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document');
```

### 匯入後資料庫狀態
```sql
SELECT FileId, ArticleId, FilePath, Type, FileName, MimeType FROM ArticleAttachments;
```
FileId                               | ArticleId                            | FilePath                                           | Type  | FileName              | MimeType
-------------------------------------|--------------------------------------|---------------------------------------------------|-------|-----------------------|----------
dddddddd-dddd-dddd-dddd-dddddddddddd | 11111111-1111-1111-1111-111111111111 | /uploads/articles/2024/01/defi-architecture-...  | image | DeFi架構圖.png         | image/png
eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee | 11111111-1111-1111-1111-111111111111 | /uploads/articles/2024/01/defi-whitepaper.pdf    | file  | DeFi白皮書.pdf         | application/pdf
ffffffff-ffff-ffff-ffff-ffffffffffff | 22222222-2222-2222-2222-222222222222 | /uploads/articles/2024/01/solidity-code-...      | image | Solidity程式碼範例.jpg  | image/jpeg
gggggggg-gggg-gggg-gggg-gggggggggggg | 22222222-2222-2222-2222-222222222222 | /uploads/articles/2024/01/smart-contract.sol     | file  | MyContract.sol        | text/plain
hhhhhhhh-hhhh-hhhh-hhhh-hhhhhhhhhhhh | 33333333-3333-3333-3333-333333333333 | /uploads/articles/2024/01/layer2-comparison-...  | image | Layer2比較圖表.png     | image/png
iiiiiiii-iiii-iiii-iiii-iiiiiiiiiiii | 33333333-3333-3333-3333-333333333333 | /uploads/articles/2024/01/layer2-technical-...   | file  | Layer2技術分析.docx    | application/vnd...

### 驗證匯入結果
```sql
-- 檢查文章附件統計
SELECT 
    Type,
    COUNT(*) AS AttachmentCount,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM ArticleAttachments), 2) AS Percentage
FROM ArticleAttachments 
GROUP BY Type
ORDER BY COUNT(*) DESC;
```
Type  | AttachmentCount | Percentage
------|-----------------|------------
image | 3               | 50.00
file  | 3               | 50.00

### 檢查文章附件詳情
```sql
-- 檢查每篇文章的附件情況
SELECT 
    SUBSTRING(a.Content, 1, 30) AS ArticlePreview,
    COUNT(att.FileId) AS AttachmentCount,
    STRING_AGG(att.Type, ', ') AS AttachmentTypes
FROM Articles a
LEFT JOIN ArticleAttachments att ON a.ArticleId = att.ArticleId
GROUP BY a.ArticleId, a.Content
ORDER BY AttachmentCount DESC;
```

## 📋 匯入注意事項

### 外鍵依賴檢查
```sql
-- 確認 ArticleId 存在於 Articles 表
SELECT COUNT(*) FROM Articles WHERE ArticleId IN ('要匯入的ArticleId列表');
```

### 必要欄位檢查
- `ArticleId`: 必須是 Articles 表中已存在的 ArticleId
- `FilePath`: 不可為空，應該是有效的檔案路徑
- `Type`: 不可為空，建議使用 "image" 或 "file"

### 檔案路徑規範
- 使用相對路徑，從網站根目錄開始
- 建議按日期或類別組織目錄結構
- 路徑格式：`/uploads/articles/YYYY/MM/filename.ext`

### MIME 類型建議
- **圖片類型**: `image/png`, `image/jpeg`, `image/gif`, `image/webp`
- **文檔類型**: `application/pdf`, `text/plain`, `application/msword`
- **Office 文檔**: `application/vnd.openxmlformats-officedocument.*`
- **壓縮檔**: `application/zip`, `application/x-rar-compressed`

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：使用不存在的 ArticleId
INSERT INTO ArticleAttachments (ArticleId, FilePath, Type) 
VALUES ('99999999-9999-9999-9999-999999999999', 
        '/uploads/test.jpg', 'image');
-- Error: FOREIGN KEY constraint failed
```

### ❌ 不合理的檔案路徑
```sql
-- 錯誤：使用絕對路徑或不安全路徑
INSERT INTO ArticleAttachments (ArticleId, FilePath, Type) 
VALUES ('11111111-1111-1111-1111-111111111111', 
        'C:\\Windows\\System32\\config', 'file');
-- Security Risk: 不安全的檔案路徑
```

### ✅ 正確範例
```sql
-- 正確：檢查文章存在後插入附件
DECLARE @ArticleId GUID = (
    SELECT TOP 1 ArticleId FROM Articles 
    WHERE Status = 0  -- 確保文章狀態正常
);

IF @ArticleId IS NOT NULL
BEGIN
    INSERT INTO ArticleAttachments (ArticleId, FilePath, Type, FileName, MimeType)
    VALUES (@ArticleId, '/uploads/articles/2024/01/example.png', 
            'image', 'example.png', 'image/png');
END
```

### 匯入順序建議
1. 先確保 Articles 表有資料且狀態正常
2. 準備附件檔案並上傳到指定目錄
3. 查詢 Articles 表取得有效的 ArticleId
4. 使用正確的檔案路徑和 MIME 類型
5. 批量插入附件記錄

### 檔案管理建議
```sql
-- 檢查孤立附件記錄（對應文章已被刪除）
SELECT att.FileId, att.FilePath
FROM ArticleAttachments att
LEFT JOIN Articles a ON att.ArticleId = a.ArticleId
WHERE a.ArticleId IS NULL OR a.Status != 0;

-- 清理孤立附件記錄
DELETE FROM ArticleAttachments 
WHERE ArticleId NOT IN (
    SELECT ArticleId FROM Articles WHERE Status = 0
);
```

### 儲存空間統計
```sql
-- 統計附件使用情況（假設知道檔案大小）
SELECT 
    Type,
    COUNT(*) AS FileCount,
    CASE Type 
        WHEN 'image' THEN COUNT(*) * 500  -- 假設平均 500KB
        WHEN 'file' THEN COUNT(*) * 2000  -- 假設平均 2MB
    END AS EstimatedSizeKB
FROM ArticleAttachments
GROUP BY Type;
```

### 常見檔案類型 MIME 對照表
```sql
-- 建立常見檔案類型參考
-- 圖片類型
-- .png -> image/png
-- .jpg, .jpeg -> image/jpeg  
-- .gif -> image/gif
-- .webp -> image/webp
-- .svg -> image/svg+xml

-- 文檔類型
-- .pdf -> application/pdf
-- .txt -> text/plain
-- .doc -> application/msword
-- .docx -> application/vnd.openxmlformats-officedocument.wordprocessingml.document
-- .xls -> application/vnd.ms-excel
-- .xlsx -> application/vnd.openxmlformats-officedocument.spreadsheetml.sheet

-- 程式碼檔案
-- .sol -> text/plain (Solidity)
-- .js -> text/javascript
-- .json -> application/json
-- .xml -> application/xml
```

### 安全性考量
- 檔案路徑應該在允許的目錄範圍內
- 檔案類型應該在白名單內
- 避免執行檔或腳本檔案
- 檔案大小應該有合理限制
- 定期檢查和清理無效檔案