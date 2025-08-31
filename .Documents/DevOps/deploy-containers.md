# 容器化部署：推到 Docker Hub → Azure Web App for Containers（超新手）

本指南帶你把 MVC 專案打包成 Docker 映像，推到 Docker Hub，然後用 Azure Web App for Containers 直接跑起來。

---

## 1. 準備條件

- 已有一個可執行的 MVC 專案（可用 `HelloMvc`）
- 安裝好 Docker Desktop（或 docker engine）
- 申請 Docker Hub 帳號（帳號名稱稍後會用）
- 申請 Azure 帳號，安裝 Azure CLI

---

## 2. 建立 Dockerfile（多階段建構）

在專案根目錄新增 `Dockerfile`：
```dockerfile
# 建置階段：還原相依並發行
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["*.csproj", "/src/"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 執行階段：只帶入發行輸出，鏡像更小
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "HelloMvc.dll"]
```

建置與本機測試：
```bash
# 將 yourhub 改成你的 Docker Hub 帳號
export HUB=yourhub

docker build -t $HUB/hellomvc:1.0 .
docker run -p 8080:80 $HUB/hellomvc:1.0
# 確認 http://localhost:8080 可開啟
```

---

## 3. 推到 Docker Hub

登入、標記、推送：
```bash
docker login
# 若本地標籤不是 $HUB/hellomvc:1.0，可再標一次
# docker tag hellomvc:1.0 $HUB/hellomvc:1.0

docker push $HUB/hellomvc:1.0
```

推送完成後，到 Docker Hub 帳號頁面，應能看到 `hellomvc:1.0` 映像。

---

## 4. 建立 Azure Web App for Containers

建立資源群組與 Web App（Linux 容器）：
```bash
az login
az group create -n hello-rg -l eastasia
az appservice plan create -g hello-rg -n hello-plan --sku B1 --is-linux

# 建立 Web App for Containers
az webapp create \
  -g hello-rg -p hello-plan \
  -n hello-mvc-container-<你的唯一名稱> \
  -i $HUB/hellomvc:1.0
```

設定啟動命令（可選）：
```bash
az webapp config set \
  -g hello-rg -n hello-mvc-container-<你的唯一名稱> \
  --startup-file "dotnet HelloMvc.dll"
```

瀏覽網站：
```bash
az webapp browse -g hello-rg -n hello-mvc-container-<你的唯一名稱>
```

---

## 5. 更新版本（發新版映像）

每次更新程式碼：
```bash
docker build -t $HUB/hellomvc:1.1 .
docker push $HUB/hellomvc:1.1

# 切換 Web App 使用的新標籤
az webapp config container set \
  -g hello-rg -n hello-mvc-container-<你的唯一名稱> \
  -i $HUB/hellomvc:1.1
```

---

## 6. 設定環境變數與連線字串

若需要（例如資料庫連線）：
```bash
az webapp config appsettings set \
  -g hello-rg -n hello-mvc-container-<你的唯一名稱> \
  --settings ConnectionStrings__Default="Data Source=/app/app.db"
```

容器內資料檔案請考慮使用持久化儲存或改連雲端資料庫（如 Azure SQL）。

---

## 7. GitHub Actions（推映像 + 更新 Web App）

在專案新增 `.github/workflows/deploy-container.yml`：
```yaml
name: Build and Push Container
on:
  push:
    branches: [ main ]

jobs:
  docker:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: docker/setup-buildx-action@v3
      - uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKERHUB_USERNAME }}
          password: ${{ secrets.DOCKERHUB_TOKEN }}
      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          push: true
          tags: ${{ secrets.DOCKERHUB_USERNAME }}/hellomvc:latest
      - name: Azure Login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      - name: Update Web App image
        run: |
          az webapp config container set \
            -g hello-rg -n hello-mvc-container-${{ secrets.AZURE_APP_SUFFIX }} \
            -i ${{ secrets.DOCKERHUB_USERNAME }}/hellomvc:latest
```

需要在 GitHub 加上這些 Secrets：
- `DOCKERHUB_USERNAME`、`DOCKERHUB_TOKEN`
- `AZURE_CREDENTIALS`（用 Azure Portal 建立 SPN / `az ad sp create-for-rbac` 產生 JSON）
- `AZURE_APP_SUFFIX`（你的唯一名稱後綴）

---

## 8. 常見問題（超新手排查）

- 拉不到映像：確認 Docker Hub 遠端名稱與標籤是否正確、映像是否公開
- 容器啟動即退出：檢查 `az webapp log tail` 或在本機 `docker run` 測試啟動命令
- 網站 500：查看應用程式日誌、確認環境變數與連線字串
- 連線字串沒生效：容器中要用 `__` 代表巢狀設定（如 `ConnectionStrings__Default`）

---

## 9. 下一步

- 使用 Azure Container Registry（ACR）取代 Docker Hub
- 建立 Staging/Production Slot 做零停機切換
- 以 Terraform/BDD 建置基礎架構
- 健康檢查與自動回滾策略

恭喜！你學會了用容器方式部署到雲端的流程！
