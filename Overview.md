# Matrix 專案摘要

## 專案說明
- 定位：Matrix 是為 Web3 先鋒與深度技術者打造的討論與創作空間，過濾主流社群的噪音，讓知識在同溫層中有序沉澱與連結。
- 精神：以鏈上身份為唯一通行證，強調專業與共識；以「燈塔／濃霧／回聲／訊號」作為意象，風格神秘、詩意、自信、精準。
- 特性：文章發布、回覆互動、標籤聚合、讚與收藏、追蹤與好友、通知流與檢舉，並支援本地化、壓縮與安全性配置。

## 主要技術（前後端）
- 後端：
  - .NET 8、ASP\.NET Core MVC + Razor Pages。
  - EF Core（SQL Server）Code First；`ApplicationDbContext` 與 `Data/Configurations/*` 管理關聯與限制。
  - AutoMapper（`Mappings/AutoMapperProfile.cs`）、MemoryCache、本地化（`zh-TW`/`en-US`）。
  - 身分驗證：JWT Bearer，Cookie 攜帶 Token（`Middleware/JwtCookieMiddleware.cs`）。
  - 安全與效能：Data Protection 金鑰持久化（`DataProtectionKeys/`）、Anti-forgery（Header: `RequestVerificationToken`）、回應壓縮（Brotli/Gzip）。
  - 郵件：`GmailService`（以 `GoogleSMTP` 設定注入）。
- 前端 / 介面：
  - Tailwind CSS + DaisyUI（透過 `tailwindcss(.exe)` 產生 `wwwroot/css/site.css`）。
  - SCSS（`wwwroot/scss/main.scss` → `wwwroot/css/components.css`）。
  - Icon 與字型：Lucide、Inter / Noto Sans TC。
  - LibMan 管理前端套件：Vue 3、Lucide、CKEditor5、date-fns、SignalR 等（`wwwroot/lib/*`）。
  - 品牌規範：深色基底 #082032、海軍藍 #2C394B、灰 #334756、重點橘 #FF4C29；圓角 8–12px、膠囊按鈕。

## 資料庫結構（主要實體與關聯）
- 使用者與個資：
  - `User(UserId, UserName, Email, Password, Role, Status, …)`。
  - `Person(PersonId, UserId FK, DisplayName, Bio, AvatarPath, BannerPath, Website1~3, IsPrivate, …)`；User 與 Person 一對一。
- 內容與互動：
  - `Article(ArticleId, AuthorId FK→Person, Content, IsPublic, Status, PraiseCount, CollectCount, CreateTime, …)`；Person 一對多 Article。
  - `Reply(ReplyId, UserId FK→Person, ArticleId FK→Article, Content, ReplyTime)`；Article 一對多 Reply。
  - `PraiseCollect(EventId, Type(0讚/1收藏), UserId FK→Person, ArticleId FK→Article, CreateTime)`；Article 一對多 PraiseCollect。
  - `ArticleAttachment(FileId, ArticleId FK→Article, FilePath, Type, FileName, MimeType)`；Article 一對多 Attachment。
  - 標籤：`Hashtag(TagId, Content, Status)`；`ArticleHashtag(ArticleId, TagId)` 為複合主鍵的中介表，Article 與 Hashtag 多對多。
- 社交關係：
  - `Follow(FollowId, UserId FK→Person, FollowedId, Type(0文章/1使用者), FollowTime)`；`FollowedId` 不設 FK，由商業邏輯解譯。
  - `Friendship(FriendshipId, UserId FK→Person, FriendId FK→Person, Status(Pending/Accepted/Declined/Blocked), RequestDate)`；雙向人際連結。
- 通知與治理：
  - `Notification(NotifyId, GetId FK→Person, SendId FK→Person, Type, IsRead, SentTime, IsReadTime)`；收發者皆為 Person。
  - `Report(ReportId, ReporterId FK→Person, TargetId, Type, Reason, Status, ResolverId FK→Person?, ProcessTime)`；TargetId 不設 FK。
- 登入稽核：
  - `LoginRecord(LoginId, UserId FK→Person, IpAddress, UserAgent, LoginTime, History)`。

關聯重點：
- Person 1–1 User、1–N Article/Reply/PraiseCollect/Follow/Notification(收發)/Report(提報/處理)/LoginRecord。
- Article 1–N Reply/PraiseCollect/Attachment，並透過 ArticleHashtag 與 Hashtag 多對多。
- Friendship 為 Person 與 Person 的雙向連結（Requester/Recipient）。
- 多處刪除行為採 Restrict 以保留歷史；Article 刪除會級聯 Reply、Attachment、PraiseCollect。

## 分層架構與模組職責
- Controllers（`Controllers/`、`Controllers/Api/`）：
  - MVC 與 API 輸入端；採薄控制器，委派服務處理商業邏輯。
  - 範圍涵蓋：Auth/Login/Register、Post/Article、Profile、Follow/Friends、Notify、Admin 等。
- Services（`Services/` + `Services/Interfaces/`）：
  - 商業邏輯核心（Article/User/Follow/Friendship/Reply/Collect/Praise/Notification/Report 等）。
  - 整合驗證、交易流程、對應與快取；對 Repository 抽象呼叫。
- Repository（`Repository/` + `Repository/Interfaces/`）：
  - EF Core 資料存取；`BaseRepository` 封裝共用存取；依實體拆分專責儲存庫。
- Data（`Data/`、`Data/Configurations/`）：
  - `ApplicationDbContext` 與 Fluent API 設定實體關聯、鍵值、刪除行為與限制。
- DTOs（`DTOs/`）與對應（`Mappings/AutoMapperProfile.cs`）：
  - 對外交握合約（輸入/輸出結構）；AutoMapper 將 DTO 與實體相互轉換。
- 視圖層（`Views/`、`ViewModels/`、`ViewComponents/`）：
  - Razor 視圖與頁面模型；View Components 規範路徑 `Views/Shared/Components/<Name>/Default.cshtml`；Partial 建議置於 `Views/Shared/`。
- 機制與擴充（`Middleware/`、`Attributes/`、`Extensions/`）：
  - 自訂中介層（JWT Cookie 注入、Dashboard 權限檢查）、授權屬性與擴充方法。
- 靜態資產（`wwwroot/`）：
  - SCSS、Tailwind、DaisyUI 與 LibMan 套件；遵循品牌色與圓角規範。

---
（開發提示）常用指令：`dotnet restore`、`dotnet build`、`dotnet run`／`dotnet watch run`；
前端開發可使用 `sass wwwroot/scss/main.scss wwwroot/css/components.css -w` 與 `tailwindcss -i wwwroot/css/tailwind.css -o wwwroot/css/site.css -w`；
LibMan：`dotnet tool run libman restore`；EF：`dotnet ef migrations add <Name>`、`dotnet ef database update`。

