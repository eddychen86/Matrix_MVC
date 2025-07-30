# LoginRecord 表假資料匯入說明

## ⚠️ 重要提醒
**PK 自動生成：** LoginRecord 表的主鍵 `LoginId` 使用 GUID，由 Entity Framework 自動生成，**請勿手動設定！**

**外鍵依賴：** `UserId` 欄位必須參照已存在的 Persons.PersonId，不可自己亂寫！

## 表格結構說明
LoginRecord 表記錄用戶的登入歷史，包括 IP 地址、瀏覽器資訊等安全相關資料。

### 欄位說明
| 欄位名 | 資料型別 | 說明 | 是否必填 | 預設值 |
|-------|---------|-----|---------|--------|
| LoginId | Guid | 登入記錄唯一識別碼 (PK) | 是 | **自動生成** |
| UserId | Guid | 登入用戶PersonId (FK) | 是 | - |
| IpAddress | string | IP 地址 | 是 | - |
| UserAgent | string | 瀏覽器用戶代理 | 是 | - |  
| LoginTime | datetime | 登入時間 | 是 | GETDATE() |
| History | string | 操作歷史記錄 | 是 | - |

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
SELECT * FROM LoginRecords;
-- (empty table)
```

### 匯入指令
```sql
-- 必須使用從 Persons 表查詢到的真實 PersonId
INSERT INTO LoginRecords (UserId, IpAddress, UserAgent, LoginTime, History)
VALUES 
    -- Alice 的登入記錄 - Windows Chrome
    ('98765432-1234-1234-1234-123456789abc', 
     '192.168.1.100', 
     'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
     DATEADD(HOUR, -2, GETDATE()),
     'Login successful, viewed dashboard, posted article about DeFi'),
     
    -- Bob 的登入記錄 - macOS Safari  
    ('98765432-1234-1234-1234-123456789def', 
     '10.0.0.50', 
     'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15',
     DATEADD(HOUR, -1, GETDATE()),
     'Login successful, replied to Alice article, updated profile'),
     
    -- Admin 的登入記錄 - Linux Firefox
    ('98765432-1234-1234-1234-123456789ghi', 
     '172.16.1.10', 
     'Mozilla/5.0 (X11; Linux x86_64; rv:120.0) Gecko/20100101 Firefox/120.0',
     DATEADD(MINUTE, -30, GETDATE()),
     'Admin login, reviewed reports, processed user feedback'),
     
    -- Alice 的第二次登入記錄 - 手機版
    ('98765432-1234-1234-1234-123456789abc', 
     '192.168.1.100', 
     'Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1',
     DATEADD(MINUTE, -10, GETDATE()),
     'Mobile login, checked notifications, liked Bob reply'),
     
    -- Bob 的第二次登入記錄 - 不同 IP
    ('98765432-1234-1234-1234-123456789def', 
     '203.69.138.45',  -- 外部 IP
     'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
     GETDATE(),
     'Login from external network, published smart contract article');
```

### 匯入後資料庫狀態
```sql
SELECT LoginId, UserId, IpAddress, 
       SUBSTRING(UserAgent, 1, 50) AS UserAgentPreview,
       LoginTime, 
       SUBSTRING(History, 1, 40) AS HistoryPreview
FROM LoginRecords
ORDER BY LoginTime DESC;
```
LoginId                              | UserId                               | IpAddress     | UserAgentPreview                         | LoginTime           | HistoryPreview
-------------------------------------|--------------------------------------|---------------|------------------------------------------|---------------------|--------------------------------
jjjjjjjj-jjjj-jjjj-jjjj-jjjjjjjjjjjj | 98765432-1234-1234-1234-123456789def | 203.69.138.45 | Mozilla/5.0 (Windows NT 10.0; Win64...  | 2024-01-01 15:00:00 | Login from external network...
kkkkkkkk-kkkk-kkkk-kkkk-kkkkkkkkkkkk | 98765432-1234-1234-1234-123456789abc | 192.168.1.100 | Mozilla/5.0 (iPhone; CPU iPhone OS...   | 2024-01-01 14:50:00 | Mobile login, checked notifications...
llllllll-llll-llll-llll-llllllllllll | 98765432-1234-1234-1234-123456789ghi | 172.16.1.10   | Mozilla/5.0 (X11; Linux x86_64; rv...   | 2024-01-01 14:30:00 | Admin login, reviewed reports...
mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm | 98765432-1234-1234-1234-123456789def | 10.0.0.50     | Mozilla/5.0 (Macintosh; Intel Mac...    | 2024-01-01 14:00:00 | Login successful, replied to Alice...
nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn | 98765432-1234-1234-1234-123456789abc | 192.168.1.100 | Mozilla/5.0 (Windows NT 10.0; Win64...  | 2024-01-01 13:00:00 | Login successful, viewed dashboard...

### 驗證匯入結果
```sql
-- 檢查用戶登入統計
SELECT 
    u.DisplayName,
    COUNT(*) AS LoginCount,
    MIN(lr.LoginTime) AS FirstLogin,
    MAX(lr.LoginTime) AS LastLogin
FROM LoginRecords lr
INNER JOIN Persons p ON lr.UserId = p.PersonId
INNER JOIN Users u ON p.UserId = u.UserId
GROUP BY p.PersonId, u.DisplayName
ORDER BY LoginCount DESC;
```
DisplayName  | LoginCount | FirstLogin          | LastLogin
-------------|------------|--------------------|--------------
Alice Chen   | 2          | 2024-01-01 13:00:00 | 2024-01-01 14:50:00
Bob Wang     | 2          | 2024-01-01 14:00:00 | 2024-01-01 15:00:00
Matrix Admin | 1          | 2024-01-01 14:30:00 | 2024-01-01 14:30:00

### 檢查 IP 地址分布
```sql
-- 檢查不同 IP 的登入情況
SELECT 
    IpAddress,
    COUNT(*) AS LoginCount,
    COUNT(DISTINCT UserId) AS UniqueUsers
FROM LoginRecords 
GROUP BY IpAddress
ORDER BY LoginCount DESC;
```

### 檢查瀏覽器使用統計
```sql
-- 簡化的瀏覽器統計
SELECT 
    CASE 
        WHEN UserAgent LIKE '%Chrome%' THEN 'Chrome'
        WHEN UserAgent LIKE '%Safari%' AND UserAgent NOT LIKE '%Chrome%' THEN 'Safari'
        WHEN UserAgent LIKE '%Firefox%' THEN 'Firefox'
        WHEN UserAgent LIKE '%Edge%' THEN 'Edge'
        ELSE 'Other'
    END AS Browser,
    COUNT(*) AS LoginCount
FROM LoginRecords
GROUP BY 
    CASE 
        WHEN UserAgent LIKE '%Chrome%' THEN 'Chrome'
        WHEN UserAgent LIKE '%Safari%' AND UserAgent NOT LIKE '%Chrome%' THEN 'Safari'
        WHEN UserAgent LIKE '%Firefox%' THEN 'Firefox'
        WHEN UserAgent LIKE '%Edge%' THEN 'Edge'
        ELSE 'Other'
    END
ORDER BY LoginCount DESC;
```

## 📋 匯入注意事項

### 外鍵依賴檢查
```sql
-- 確認 UserId 存在於 Persons 表
SELECT COUNT(*) FROM Persons WHERE PersonId IN ('要匯入的UserId列表');
```

### 必要欄位檢查
- `UserId`: 必須是 Persons 表中已存在的 PersonId
- `IpAddress`: 有效的 IPv4 或 IPv6 地址格式
- `UserAgent`: 真實的瀏覽器用戶代理字串
- `History`: 具體的操作記錄描述

### IP 地址格式
- **內網 IP**: `192.168.x.x`, `10.x.x.x`, `172.16-31.x.x`
- **外網 IP**: 公共 IPv4 地址
- **IPv6**: 支援但較少使用

### UserAgent 格式參考
```sql
-- 常見 UserAgent 範例
-- Windows Chrome:
'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'

-- macOS Safari:
'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15'

-- Linux Firefox:
'Mozilla/5.0 (X11; Linux x86_64; rv:120.0) Gecko/20100101 Firefox/120.0'

-- iPhone Safari:
'Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1'

-- Android Chrome:
'Mozilla/5.0 (Linux; Android 13; SM-G998B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36'
```

## 🚨 常見錯誤避免

### ❌ 錯誤範例
```sql
-- 錯誤：使用不存在的 UserId
INSERT INTO LoginRecords (UserId, IpAddress, UserAgent, History) 
VALUES ('99999999-9999-9999-9999-999999999999', 
        '192.168.1.1', 'Chrome', 'Login test');
-- Error: FOREIGN KEY constraint failed
```

### ❌ 不合理的資料
```sql
-- 錯誤：不真實的 IP 和 UserAgent
INSERT INTO LoginRecords (UserId, IpAddress, UserAgent, History) 
VALUES ('98765432-1234-1234-1234-123456789abc', 
        '999.999.999.999', 'MyBrowser', 'Did something');
-- Data Quality Issue: 無效的 IP 格式和不真實的 UserAgent
```

### ✅ 正確範例
```sql
-- 正確：建立真實的登入記錄
DECLARE @UserId GUID = (
    SELECT PersonId FROM Persons 
    INNER JOIN Users ON Persons.UserId = Users.UserId 
    WHERE Users.UserName = 'alice_chen'
);

INSERT INTO LoginRecords (UserId, IpAddress, UserAgent, LoginTime, History)
VALUES (@UserId, 
        '192.168.1.100', 
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
        GETDATE(),
        'Successful login, browsed articles, posted new content');
```

### 匯入順序建議
1. 先確保 Persons 表有資料
2. 查詢 Persons 表取得有效的 PersonId
3. 使用真實的 IP 地址和 UserAgent
4. 設定合理的登入時間序列
5. 記錄具體的操作歷史

### 安全性分析查詢
```sql
-- 檢查可疑登入活動
-- 1. 短時間內多次登入
SELECT 
    p.DisplayName,
    COUNT(*) AS LoginCount,
    MIN(lr.LoginTime) AS FirstLogin,
    MAX(lr.LoginTime) AS LastLogin,
    DATEDIFF(MINUTE, MIN(lr.LoginTime), MAX(lr.LoginTime)) AS TimeSpanMinutes
FROM LoginRecords lr
INNER JOIN Persons p ON lr.UserId = p.PersonId
INNER JOIN Users u ON p.UserId = u.UserId
WHERE lr.LoginTime >= DATEADD(HOUR, -1, GETDATE())
GROUP BY p.PersonId, p.DisplayName
HAVING COUNT(*) > 3;

-- 2. 異常 IP 登入
SELECT 
    lr.UserId,
    u.DisplayName,
    lr.IpAddress,
    COUNT(*) AS LoginCount
FROM LoginRecords lr
INNER JOIN Persons p ON lr.UserId = p.PersonId
INNER JOIN Users u ON p.UserId = u.UserId
GROUP BY lr.UserId, u.DisplayName, lr.IpAddress
HAVING COUNT(*) = 1  -- 只登入一次的 IP（可能是新設備）
ORDER BY lr.UserId;
```

### 數據清理建議
```sql
-- 清理過舊的登入記錄（保留最近 90 天）
DELETE FROM LoginRecords 
WHERE LoginTime < DATEADD(DAY, -90, GETDATE());

-- 檢查數據完整性
SELECT 
    'Missing IP' AS IssueType,
    COUNT(*) AS Count
FROM LoginRecords 
WHERE IpAddress IS NULL OR IpAddress = ''
UNION ALL
SELECT 
    'Missing UserAgent',
    COUNT(*)
FROM LoginRecords 
WHERE UserAgent IS NULL OR UserAgent = ''
UNION ALL
SELECT 
    'Missing History',
    COUNT(*)
FROM LoginRecords 
WHERE History IS NULL OR History = '';
```

### 登入統計報表
```sql
-- 每日登入統計
SELECT 
    CAST(LoginTime AS DATE) AS LoginDate,
    COUNT(*) AS LoginCount,
    COUNT(DISTINCT UserId) AS UniqueUsers
FROM LoginRecords 
WHERE LoginTime >= DATEADD(DAY, -30, GETDATE())
GROUP BY CAST(LoginTime AS DATE)
ORDER BY LoginDate DESC;
```