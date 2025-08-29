# 部署與設定管理技術文件

**技術分類**: DevOps 與部署管理  
**複雜度**: 中級到高級  
**適用情境**: 生產環境部署、設定管理、CI/CD 流程  

## 技術概述

Matrix 專案採用多環境部署策略，支援 Development、Production 等環境，並整合 Azure 雲端服務與本地開發環境的無縫切換。

## 基礎技術

### 1. 專案設定架構
```
Matrix/
├── appsettings.json              # 基礎設定
├── appsettings.Development.json  # 開發環境設定
├── appsettings.Production.json   # 生產環境設定
├── global.json                   # .NET SDK 版本設定
├── Properties/
│   ├── launchSettings.json       # 啟動設定
│   ├── PublishProfiles/          # 發布設定檔
│   └── serviceDependencies.json  # 服務依賴設定
└── DataProtectionKeys/           # 資料保護金鑰存放
```

### 2. 環境設定管理 (appsettings.json:1-24)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "",           // 預設連線字串
    "AzureConnection": ""             // Azure SQL 連線字串
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "JWT": {
    "Key": "",                        // JWT 金鑰 (透過 User Secrets)
    "Issuer": ""                      // JWT 發行者
  },
  "AllowedHosts": "*",
  "GoogleSmtp": {
    "SenderEmail": "",                // 發送者信箱
    "AppPassword": "",                // 應用程式密碼
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true
  }
}
```

### 3. .NET SDK 版本控制 (global.json:1-7)
```json
{
  "sdk": {
    "version": "8.0.0",               // 指定 SDK 版本
    "rollForward": "latestMinor",     // 版本升級策略
    "allowPrerelease": false          // 不允許預發行版本
  }
}
```

## 進階設定管理

### 1. 啟動設定檔 (Properties/launchSettings.json:1-38)
```json
{
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:6447",
      "sslPort": 44315
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5002",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### 2. 動態端口配置 (Program.cs:258-278)
```csharp
// 動態端口偵測與配置
var originalUrls = builder.Configuration["Urls"];
if (!string.IsNullOrEmpty(originalUrls))
{
    var uri = new Uri(originalUrls);
    var originalPort = uri.Port;
    var availablePort = FindAvailablePort(originalPort);

    if (availablePort != originalPort)
    {
        var newUrl = $"{uri.Scheme}://{uri.Host}:{availablePort}";
        builder.WebHost.UseUrls(newUrl);
        Console.WriteLine($"原始端口 {originalPort} 已被占用，改用端口 {availablePort}");
    }
}

// 端口可用性檢查
private static bool IsPortAvailable(int port)
{
    try
    {
        var tcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
        return !tcpListeners.Any(listener => listener.Port == port);
    }
    catch
    {
        return false;
    }
}
```

### 3. 資料保護金鑰管理 (Program.cs:63-68)
```csharp
// DataProtection 金鑰持久化配置
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))  // 檔案系統存放
    .SetApplicationName("Matrix");                         // 應用程式名稱
```

## 環境配置策略

### 1. Development 環境設定
```json
{
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Matrix_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 2. Production 環境設定
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  },
  "AllowedHosts": "*.azurewebsites.net;yourdomain.com"
}
```

### 3. User Secrets 管理
```bash
# 初始化 User Secrets
dotnet user-secrets init

# 設定敏感資料
dotnet user-secrets set "JWT:Key" "your-secret-jwt-key"
dotnet user-secrets set "ConnectionStrings:AzureConnection" "your-azure-connection-string"
dotnet user-secrets set "GoogleSmtp:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "GoogleSmtp:AppPassword" "your-app-password"

# 查看所有 Secrets
dotnet user-secrets list
```

## 部署配置

### 1. Azure App Service 發布設定檔
```xml
<!-- Properties/PublishProfiles/web3matrix - Web Deploy.pubxml -->
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <WebPublishMethod>MSDeploy</WebPublishMethod>
    <PublishProvider>AzureWebSite</PublishProvider>
    <LastUsedBuildConfiguration>Release</LastUsedBuildConfiguration>
    <LastUsedPlatform>Any CPU</LastUsedPlatform>
    <SiteUrlToLaunchAfterPublish>https://web3matrix.azurewebsites.net</SiteUrlToLaunchAfterPublish>
    <LaunchSiteAfterPublish>True</LaunchSiteAfterPublish>
    <ProjectGuid>12345678-1234-1234-1234-123456789abc</ProjectGuid>
    <PublishUrl>web3matrix.scm.azurewebsites.net:443</PublishUrl>
    <UserName>$web3matrix</UserName>
    <_SavePWD>True</_SavePWD>
  </PropertyGroup>
</Project>
```

### 2. Docker 容器化設定
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Matrix.csproj", "."]
RUN dotnet restore "Matrix.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Matrix.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Matrix.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Matrix.dll"]
```

### 3. Docker Compose 設定
```yaml
# docker-compose.yml
version: '3.8'

services:
  matrix-app:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
      - "8443:443"
    depends_on:
      - matrix-db
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=matrix-db;Database=Matrix;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true
    volumes:
      - ./DataProtectionKeys:/app/DataProtectionKeys

  matrix-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"
    environment:
      - SA_PASSWORD=${DB_PASSWORD}
      - ACCEPT_EULA=Y
    volumes:
      - matrix-db-data:/var/opt/mssql

volumes:
  matrix-db-data:
```

## CI/CD 管道設定

### 1. GitHub Actions 工作流程
```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Set up .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal
      
    - name: Publish
      run: dotnet publish --no-build --configuration Release --output ./publish
      
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'web3matrix'
        slot-name: 'Production'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: './publish'
```

### 2. Azure DevOps Pipeline
```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
    - main
    - develop

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'
  dotnetSdkVersion: '8.0.x'

stages:
- stage: Build
  displayName: 'Build Stage'
  jobs:
  - job: Build
    displayName: 'Build Job'
    steps:
    - task: UseDotNet@2
      displayName: 'Use .NET SDK $(dotnetSdkVersion)'
      inputs:
        version: '$(dotnetSdkVersion)'
        
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: '**/*.csproj'
        
    - task: DotNetCoreCLI@2
      displayName: 'Build application'
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration) --no-restore'
        
    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: '**/*Tests.csproj'
        arguments: '--configuration $(buildConfiguration) --no-build'
        
    - task: DotNetCoreCLI@2
      displayName: 'Publish application'
      inputs:
        command: 'publish'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        
    - task: PublishBuildArtifacts@1
      displayName: 'Publish artifacts'
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'drop'

- stage: Deploy
  displayName: 'Deploy Stage'
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: Deploy
    displayName: 'Deploy Job'
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            displayName: 'Deploy to Azure Web App'
            inputs:
              azureSubscription: '$(azureSubscription)'
              appName: 'web3matrix'
              package: '$(Pipeline.Workspace)/drop'
```

## 設定驗證與測試

### 1. 設定驗證機制
```csharp
// Startup 階段設定驗證
public class ConfigurationValidator
{
    public static void ValidateConfiguration(IConfiguration configuration)
    {
        var requiredSettings = new[]
        {
            "ConnectionStrings:DefaultConnection",
            "JWT:Key",
            "JWT:Issuer"
        };

        var missingSettings = new List<string>();

        foreach (var setting in requiredSettings)
        {
            if (string.IsNullOrEmpty(configuration[setting]))
            {
                missingSettings.Add(setting);
            }
        }

        if (missingSettings.Any())
        {
            throw new InvalidOperationException(
                $"Missing required configuration settings: {string.Join(", ", missingSettings)}");
        }
    }
}

// 在 Program.cs 中使用
try
{
    ConfigurationValidator.ValidateConfiguration(builder.Configuration);
}
catch (InvalidOperationException ex)
{
    // 記錄錯誤並優雅退出
    Console.WriteLine($"Configuration Error: {ex.Message}");
    Environment.Exit(1);
}
```

### 2. 健康檢查設定
```csharp
// 健康檢查服務註冊
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddSqlServer(connectionString, name: "database")
    .AddCheck<EmailServiceHealthCheck>("email-service")
    .AddCheck<ExternalApiHealthCheck>("external-api");

// 健康檢查端點
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                exception = entry.Value.Exception?.Message,
                duration = entry.Value.Duration.ToString()
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

### 3. 效能監控設定
```csharp
// Application Insights 整合
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

// 自定義效能監控
builder.Services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();

public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await _next(context);
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 1000) // 記錄超過 1 秒的請求
        {
            _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMilliseconds}ms", 
                context.Request.Method, 
                context.Request.Path, 
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 實際應用情境

### 1. 多環境資料庫連線策略
```csharp
// 動態連線字串選擇
public class DatabaseConnectionService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public string GetConnectionString()
    {
        if (_environment.IsDevelopment())
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }
        else if (_environment.IsProduction())
        {
            // 優先使用 Azure 連線字串
            var azureConnection = _configuration.GetConnectionString("AzureConnection");
            if (!string.IsNullOrEmpty(azureConnection))
            {
                return azureConnection;
            }
        }

        throw new InvalidOperationException("No valid database connection string found");
    }
}
```

### 2. 功能開關管理
```csharp
// 功能開關設定
public class FeatureFlags
{
    public bool EnableNewDashboard { get; set; }
    public bool EnableAdvancedSearch { get; set; }
    public bool EnableBetaFeatures { get; set; }
}

// 註冊功能開關
builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection("FeatureFlags"));

// 使用功能開關
public class HomeController : Controller
{
    private readonly IOptions<FeatureFlags> _featureFlags;

    public IActionResult Index()
    {
        if (_featureFlags.Value.EnableNewDashboard)
        {
            return View("NewDashboard");
        }
        
        return View("ClassicDashboard");
    }
}
```

### 3. 自動化備份與還原
```bash
#!/bin/bash
# backup-database.sh

# 設定變數
BACKUP_DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_PATH="/backups/matrix_${BACKUP_DATE}.bak"
CONNECTION_STRING="Server=localhost;Database=Matrix;Integrated Security=true"

# 建立備份
echo "開始資料庫備份..."
sqlcmd -S localhost -d Matrix -Q "BACKUP DATABASE [Matrix] TO DISK = '${BACKUP_PATH}'"

# 驗證備份
if [ -f "$BACKUP_PATH" ]; then
    echo "備份完成: $BACKUP_PATH"
    
    # 上傳到 Azure Blob Storage
    az storage blob upload \
        --account-name "matrixbackups" \
        --container-name "database-backups" \
        --name "matrix_${BACKUP_DATE}.bak" \
        --file "$BACKUP_PATH"
else
    echo "備份失敗!"
    exit 1
fi
```

---

**建立日期**: 2025-08-29  
**適用環境**: Azure App Service, Docker, 本地開發  
**相關檔案**: appsettings*.json, global.json, launchSettings.json  
**CI/CD 工具**: GitHub Actions, Azure DevOps  
**學習資源**: [ASP.NET Core 部署](https://docs.microsoft.com/aspnet/core/host-and-deploy/), [Azure App Service](https://docs.microsoft.com/azure/app-service/)