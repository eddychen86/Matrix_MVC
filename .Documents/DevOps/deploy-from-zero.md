# 從零部署到雲端（超新手友善）

這份指南帶你從「完全沒經驗」一路到「把 MVC 網站部署到雲端」。每個步驟都很短、可以邊做邊看成果。

---

## 1. 先在本機跑起來

- 有 .NET 8 SDK 和 VS Code
- 在任意資料夾開啟終端機：
```
 dotnet new mvc -n HelloMvc
 cd HelloMvc
 dotnet run
```
- 確認瀏覽器可開 `http://localhost:5000`（或終端機顯示的網址）

---

## 2. 加上 SQLite（讓網站有資料）

- 安裝套件：
```
 dotnet add package Microsoft.EntityFrameworkCore.Sqlite
 dotnet add package Microsoft.EntityFrameworkCore.Design
```
- 建 `Models/Post.cs` 與 `Data/AppDbContext.cs`（可參考 Backend/mvc-from-zero.md）
- 建立資料表：
```
 dotnet ef migrations add Init
 dotnet ef database update
```
- 再 `dotnet run`，網站應可正常運作

---

## 3. 用 Docker 跑起來（可選，但很有用）

新增 `Dockerfile`：
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HelloMvc.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "HelloMvc.dll"]
```

建映像並啟動容器：
```
 docker build -t hello-mvc:1.0 .
 docker run -p 8080:80 hello-mvc:1.0
```
在瀏覽器打 `http://localhost:8080`

---

## 4. 部署到 Azure（最簡單路線）

前置：
- 註冊 Azure 帳號（有免費額度）
- 安裝 Azure CLI

登入與建立 App Service：
```
 az login
 az group create -n hello-rg -l eastasia
 az appservice plan create -g hello-rg -n hello-plan --sku B1 --is-linux
 az webapp create -g hello-rg -p hello-plan -n hello-mvc-<你的唯一名稱> --runtime "DOTNET|8.0"
```

部署（zip 部署最簡單）：
```
 dotnet publish -c Release -o publish
 cd publish
 zip -r app.zip .
 az webapp deploy --resource-group hello-rg --name hello-mvc-<你的唯一名稱> --src-path app.zip --type zip
```

打開網站：
```
 az webapp browse -g hello-rg -n hello-mvc-<你的唯一名稱>
```

---

## 5. 設定機密（連線字串等）

- 在本機開發：用 `dotnet user-secrets` 儲存機密
```
 dotnet user-secrets init
 dotnet user-secrets set "ConnectionStrings:Default" "Data Source=app.db"
```
- 在 Azure：用 App Settings 設定 Key-Value（等同環境變數）
  - 入口：Azure Portal → Web App → Configuration → Application settings
  - 範例：`ConnectionStrings__Default = <你的連線字串>`

---

## 6. 自動化部署（GitHub Actions）

在專案根目錄新增 `.github/workflows/deploy.yml`：
```yaml
name: Build and Deploy
on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Build
      run: |
        dotnet restore
        dotnet build -c Release
        dotnet publish -c Release -o publish
    - name: Zip
      run: cd publish && zip -r app.zip .
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: hello-mvc-<你的唯一名稱>
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: publish/app.zip
```

- 在 GitHub 專案 Settings → Secrets → Actions 新增 `AZURE_WEBAPP_PUBLISH_PROFILE`
- 之後 push 到 main 就會自動部署

---

## 7. 健康檢查與監控（入門）

- 在 `Program.cs` 加入 Health Checks：
```csharp
builder.Services.AddHealthChecks();
app.MapHealthChecks("/health");
```
- 上線後打 `https://你的網站.azurewebsites.net/health` 看狀態
- 也可以在 Azure Portal 啟用「Application Insights」觀察錯誤與效能

---

## 8. 常見問題（超新手排查）

- 網站跑起來是 500 錯誤：檢查 Azure App Settings 是否缺少連線字串或環境變數
- 無法部署：確認 `hello-mvc-<你的唯一名稱>` 在全球是唯一的
- Docker 容器無法啟動：`docker logs <容器ID>` 看錯誤訊息
- 看到大小寫錯誤（C#）：確認屬性名稱大小寫，例如 `DisplayName`/`displayName`

---

## 9. 下一步

- 把 SQLite 換成 Azure SQL
- 建立 Staging/Production Slot 做零停機部署
- 自動備份與回滾策略
- 使用 Key Vault 管理機密

恭喜！你已經把一個 MVC 網站從本機一路部署到雲端了！
