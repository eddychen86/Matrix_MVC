# Hashtag 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** Hashtag 表的主鍵 `TagId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**無外鍵依賴：** 此表是基礎資料表，不依賴其他表，但會被 ArticleHashtag 表參照。

## 表格結構說明
Hashtag 表存儲系統中的標籤資料，用於分類和標記文章內容，支援 Web3 和技術相關主題。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| TagId | Guid | 標籤唯一識別碼 (PK) | 是 | **自動生成** |
| Content | string(10) | 標籤內容 | 是 | - |
| Status | int | 標籤狀態 (0=正常) | 是 | 0 |

### 標籤狀態說明
- `Status = 0`: 正常使用
- `Status = 1`: 已停用
- `Status = 2`: 已刪除

## 🔧 假資料匯入範例

### 匯入前資料庫狀態
```sql
SELECT * FROM Hashtags;
-- (empty table)
```

### 匯入指令
```sql
-- 插入 Web3 和技術相關標籤
INSERT INTO Hashtags (Content, Status)
VALUES 
    ('Web3', 0),
    ('DeFi', 0),
    ('Blockchain', 0),
    ('Smart Contract', 0),  -- 注意：字串長度限制 10 字元
    ('NFT', 0),
    ('Ethereum', 0),
    ('Bitcoin', 0),
    ('Solidity', 0),
    ('Layer2', 0),
    ('Polygon', 0),
    ('Arbitrum', 0),
    ('Optimism', 0),
    ('React', 0),
    ('Node.js', 0),  -- 注意：會被截斷為 'Node.js'（7字元）
    ('TypeScript', 0),  -- 注意：會被截斷為 'TypeScript'（10字元）
    ('JavaScript', 0),  -- 注意：會被截斷為 'JavaScript'（10字元）
    ('Python', 0),
    ('AI', 0),
    ('MachineLearning', 0);  -- 注意：超過10字元，會被截斷
```

### 匯入後資料庫狀態
```sql
SELECT TagId, Content, Status FROM Hashtags ORDER BY Content;
```
TagId                                | Content    | Status
-------------------------------------|------------|--------
tttttttt-tttt-tttt-tttt-tttttttttttt | AI         | 0
uuuuuuuu-uuuu-uuuu-uuuu-uuuuuuuuuuuu | Arbitrum   | 0
vvvvvvvv-vvvv-vvvv-vvvv-vvvvvvvvvvvv | Bitcoin    | 0
wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww | Blockchain | 0
xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx | DeFi       | 0
yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy | Ethereum   | 0
zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz | JavaScrip  | 0  -- 被截斷
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | Layer2     | 0
bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | MachineLea | 0  -- 被截斷
cccccccc-cccc-cccc-cccc-cccccccccccc | NFT        | 0
dddddddd-dddd-dddd-dddd-dddddddddddd | Node.js    | 0
eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee | Optimism   | 0
ffffffff-ffff-ffff-ffff-ffffffffffff | Polygon    | 0
gggggggg-gggg-gggg-gggg-gggggggggggg | Python     | 0
hhhhhhhh-hhhh-hhhh-hhhh-hhhhhhhhhhhh | React      | 0
iiiiiiii-iiii-iiii-iiii-iiiiiiiiiiii | Smart Cont | 0  -- 被截斷
jjjjjjjj-jjjj-jjjj-jjjj-jjjjjjjjjjjj | Solidity   | 0
kkkkkkkk-kkkk-kkkk-kkkk-kkkkkkkkkkkk | TypeScript | 0
llllllll-llll-llll-llll-llllllllllll | Web3       | 0

### 正確的匯入指令（考慮字元限制）
```sql
-- 重新匯入，確保所有標籤都在 10 字元以內
DELETE FROM Hashtags;  -- 清空後重新匯入

INSERT INTO Hashtags (Content, Status)
VALUES 
    ('Web3', 0),
    ('DeFi', 0),
    ('Blockchain', 0),
    ('SmartContract', 0),  -- 調整為 10 字元以內
    ('NFT', 0),
    ('Ethereum', 0),
    ('Bitcoin', 0),
    ('Solidity', 0),
    ('Layer2', 0),
    ('Polygon', 0),
    ('Arbitrum', 0),
    ('Optimism', 0),
    ('React', 0),
    ('NodeJS', 0),      -- 調整為 'NodeJS'
    ('TypeScript', 0),  -- 10 字元正好
    ('JavaScript', 0),  -- 10 字元正好
    ('Python', 0),
    ('AI', 0),
    ('MachineLrn', 0);  -- 調整為 'MachineLrn'
```

## 📋 匯入注意事項

### 字元長度限制
- `Content`: 最大長度 **10 字元**
- 超過 10 字元的內容會被資料庫截斷
- 建議事先檢查標籤長度

### 必要欄位檢查
- `Content`: 不可為空，且應該有意義
- `Status`: 預設值為 0（正常）

### 資料完整性規則
- 標籤內容應該唯一（業務邏輯）
- 標籤名稱應該符合平台主題（Web3、技術相關）
- 避免特殊字元和表情符號

### 標籤命名建議
- 使用技術相關詞彙
- 避免過於通用的詞彙
- 考慮中英文混合使用
- 優先使用業界標準術語

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：標籤內容超過 10 字元
INSERT INTO Hashtags (Content, Status) 
VALUES ('Smart Contract Development', 0);
-- 會被截斷為 'Smart Cont'
```

### ❌ 重複標籤
```sql
-- 錯誤：可能建立重複標籤（業務邏輯）
INSERT INTO Hashtags (Content, Status) 
VALUES ('Web3', 0), ('web3', 0), ('WEB3', 0);
-- 雖然資料庫允許，但業務邏輯上應該避免
```

### ✅ 正確範例
```sql
-- 正確：檢查標籤是否已存在
IF NOT EXISTS (SELECT 1 FROM Hashtags WHERE LOWER(Content) = 'web3')
BEGIN
    INSERT INTO Hashtags (Content, Status) VALUES ('Web3', 0);
END

-- 正確：批量插入唯一標籤
WITH NewTags AS (
    SELECT 'Web3' AS TagName, 0 AS Status
    UNION SELECT 'DeFi', 0
    UNION SELECT 'Blockchain', 0
)
INSERT INTO Hashtags (Content, Status)
SELECT TagName, Status FROM NewTags nt
WHERE NOT EXISTS (
    SELECT 1 FROM Hashtags h 
    WHERE LOWER(h.Content) = LOWER(nt.TagName)
);
```

### 匯入順序建議
1. 清理現有重複標籤（如需要）
2. 準備標籤清單，確保都在 10 字元以內
3. 檢查標籤唯一性
4. 批量匯入標籤
5. 驗證匯入結果

### 標籤管理建議
```sql
-- 檢查字元長度超限的標籤
SELECT Content, LEN(Content) AS Length
FROM Hashtags 
WHERE LEN(Content) > 10;

-- 檢查重複標籤（忽略大小寫）
SELECT LOWER(Content) AS LowerContent, COUNT(*) AS Count
FROM Hashtags 
GROUP BY LOWER(Content)
HAVING COUNT(*) > 1;

-- 清理重複標籤（保留最早建立的）
WITH DuplicateTags AS (
    SELECT TagId, Content,
           ROW_NUMBER() OVER (PARTITION BY LOWER(Content) ORDER BY TagId) AS RowNum
    FROM Hashtags
)
DELETE FROM Hashtags 
WHERE TagId IN (
    SELECT TagId FROM DuplicateTags WHERE RowNum > 1
);
```

### Web3 主題標籤建議清單
```sql
-- 推薦的 Web3 技術標籤
INSERT INTO Hashtags (Content, Status) VALUES
('Web3', 0),         -- Web3 總稱
('DeFi', 0),         -- 去中心化金融
('NFT', 0),          -- 非同質化代幣
('DAO', 0),          -- 去中心化自治組織
('Dapp', 0),         -- 去中心化應用
('Ethereum', 0),     -- 以太坊
('Bitcoin', 0),      -- 比特幣
('Solidity', 0),     -- 智能合約語言
('Layer2', 0),       -- 第二層解決方案
('Polygon', 0),      -- Polygon 網路
('Arbitrum', 0),     -- Arbitrum 網路
('Optimism', 0),     -- Optimism 網路
('DeFiTokens', 0),   -- DeFi 代幣
('Staking', 0),      -- 質押
('Yield', 0),        -- 收益挖礦
('AMM', 0),          -- 自動做市商
('CEX', 0),          -- 中心化交易所
('DEX', 0),          -- 去中心化交易所
('Wallet', 0),       -- 錢包
('MetaMask', 0);     -- MetaMask 錢包
```