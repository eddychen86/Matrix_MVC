# DevOps 技術文件索引

**分類**: DevOps & Deployment  
**技術領域**: 部署自動化、環境管理、CI/CD 流程  

## 📋 文件列表

### 文件 1: 部署與設定管理完整指南
**檔案**: [`deployment-config.md`](./deployment-config.md)  
**描述**: 多環境部署策略、Azure 雲端整合、Docker 容器化、CI/CD 自動化流程完整解析  
**關鍵字**: Azure App Service, Docker, CI/CD, 環境設定, User Secrets, GitHub Actions, 健康檢查  
**相關檔案**: appsettings*.json, Dockerfile, azure-pipelines.yml, .github/workflows/  
**複雜度**: 中級到高級  

**內容概要**:
- 多環境設定管理 (Dev/Prod)
- Azure App Service 部署
- Docker 容器化設定
- GitHub Actions CI/CD 流程
- 健康檢查與監控
- 安全設定管理
- 效能監控整合

---

## 🎯 學習路線

### 入門階段 (1-2 週)
1. **環境概念**: 理解 Development/Production 環境差異
2. **設定管理**: 學習 appsettings.json 階層式設定
3. **User Secrets**: 掌握敏感資料的安全管理

### 進階階段 (2-3 週)  
1. **容器化**: 學習 Docker 基礎和 Dockerfile 編寫
2. **雲端部署**: 掌握 Azure App Service 部署流程
3. **CI/CD**: 建立自動化部署管道

### 專家階段 (2-3 週)
1. **監控整合**: 實作健康檢查和效能監控
2. **多環境管理**: 建立複雜的環境管理策略
3. **故障排除**: 學習生產環境問題診斷

---

## 🔗 技術關聯

### 雲端服務
- **Azure App Service**: 主要部署平台
- **Azure SQL Database**: 雲端資料庫
- **Azure Application Insights**: 監控和診斷
- **Azure Key Vault**: 金鑰管理服務

### 容器技術
- **Docker**: 應用程式容器化
- **Docker Compose**: 多服務編排
- **Container Registry**: 映像檔管理

### CI/CD 工具
- **GitHub Actions**: 自動化工作流程
- **Azure DevOps**: 企業級 DevOps 平台
- **Docker Hub**: 公共映像檔倉庫

---

## 🚀 部署架構

### 環境層級
```
Development Environment
├── Local Development (localhost)
├── Docker Development (容器測試)
└── Azure Development Slot

Production Environment  
├── Azure Production Slot (主要)
├── Azure Staging Slot (預發佈)
└── Container Production (備援)
```

### 設定檔層級
```
appsettings.json (基礎設定)
├── appsettings.Development.json (開發環境)
├── appsettings.Production.json (生產環境)
├── User Secrets (敏感資料)
└── Environment Variables (雲端設定)
```

---

## 💡 最佳實務

### 設定管理
- **階層式設定**: 基礎 → 環境特定 → 秘密資料
- **環境變數**: 雲端部署時使用環境變數覆蓋
- **金鑰輪替**: 定期更新敏感資料
- **設定驗證**: 啟動時驗證必要設定

### 部署策略
- **藍綠部署**: 使用 Azure Slot 實現零停機部署
- **滾動更新**: Docker Swarm 或 Kubernetes 滾動更新
- **回滾機制**: 快速回滾到前一版本的能力
- **健康檢查**: 部署後自動驗證應用程式狀態

### 監控與告警
```csharp
// 健康檢查端點
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

---

## 🔧 實作工具

### 必要工具
- **Azure CLI**: 雲端資源管理
- **Docker Desktop**: 本地容器開發
- **Git**: 版本控制
- **dotnet CLI**: .NET 應用程式管理

### 推薦工具
- **Azure Data Studio**: 資料庫管理
- **Postman**: API 測試
- **k6**: 效能測試
- **Azure Monitor**: 應用程式監控

### 開發環境設定
```bash
# 安裝必要工具
# Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Docker (Ubuntu)
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io

# .NET SDK
wget https://dot.net/v1/dotnet-install.sh
bash dotnet-install.sh --version 8.0.0
```

---

## 📊 部署流程範例

### GitHub Actions 工作流程
```yaml
name: Deploy to Azure
on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Build and Test
      run: |
        dotnet restore
        dotnet build --configuration Release
        dotnet test --configuration Release
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'matrix-app'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
```

### Docker 多階段建構
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Matrix.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish  
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "Matrix.dll"]
```

---

## 📚 推薦學習資源

### Microsoft 官方文件
- [Azure App Service 部署](https://docs.microsoft.com/azure/app-service/)
- [ASP.NET Core 部署](https://docs.microsoft.com/aspnet/core/host-and-deploy/)
- [Docker 容器化指南](https://docs.microsoft.com/dotnet/core/docker/)

### 最佳實務指南
- [12-Factor App](https://12factor.net/)
- [Azure Architecture Center](https://docs.microsoft.com/azure/architecture/)
- [DevOps 實務指南](https://docs.microsoft.com/devops/)

---

**最後更新**: 2025-08-29  
**文件數量**: 1  
**總學習時間**: 5-8 週 (依個人基礎而定)