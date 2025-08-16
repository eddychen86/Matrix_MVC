using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Matrix.Middleware;
using Matrix.Services;
using Matrix.Controllers;
using Matrix.Data;
using Matrix.Repository;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;
using Matrix.Models;
using Matrix.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
// using Microsoft.AspNetCore.Identity;

namespace Matrix;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 配置 Console Logging Provider
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // 從配置中獲取連接字串 (會自動從 appsettings.json, secrets.json, 環境變數等來源載入)
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection connection string is not configured.");
        }

        // Add services to the container.
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions => 
            {
                sqlOptions.CommandTimeout(60); // 增加到 60 秒
                sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null); // 啟用重試機制
            }));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // DataProtection 金鑰持久化，避免重啟後 Cookie/Antiforgery 失效
        var keysPath = System.IO.Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
        System.IO.Directory.CreateDirectory(keysPath);
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new System.IO.DirectoryInfo(keysPath))
            .SetApplicationName("Matrix");

        #region 註冊 Repository

        // 註冊 Repository
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPersonRepository, PersonRepository>();
        builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
        builder.Services.AddScoped<IReplyRepository, ReplyRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<IFollowRepository, FollowRepository>();
        builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        builder.Services.AddScoped<IPraiseCollectRepository, PraiseCollectRepository>();
        builder.Services.AddScoped<IReportRepository, ReportRepository>();
        builder.Services.AddScoped<IHashtagRepository, HashtagRepository>();
        builder.Services.AddScoped<IArticleAttachmentRepository, ArticleAttachmentRepository>();
        builder.Services.AddScoped<IArticleHashtagRepository, ArticleHashtagRepository>();
        builder.Services.AddScoped<ILoginRecordRepository, LoginRecordRepository>();
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        
        #endregion

        #region 註冊 Service

        // 註冊記憶體快取
        builder.Services.AddMemoryCache();
        
        // 註冊 AutoMapper
        builder.Services.AddAutoMapper(cfg => {
            cfg.AddProfile<Matrix.Mappings.AutoMapperProfile>();
        });

        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
        builder.Services.AddScoped<ICollectService, CollectService>();
        builder.Services.AddScoped<IPraiseService, PraiseService>();
        builder.Services.AddScoped<IReplyService, ReplyService>();
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<IHashtagService, HashtagService>();
        builder.Services.AddScoped<IArticleService, ArticleService>();
        builder.Services.AddScoped<ISystemStatusService, SystemStatusService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<Matrix.Controllers.AuthController>();
        builder.Services.AddHttpContextAccessor(); // 為 CustomLocalizer 提供 HttpContext 訪問
        builder.Services.AddScoped<ICustomLocalizer, CustomLocalizer>();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.AddScoped<IArticleService, ArticleService>();

        // 配置本地化選項
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "zh-TW", "en-US" };
            options.SetDefaultCulture("zh-TW")
                   .AddSupportedCultures(supportedCultures)
                   .AddSupportedUICultures(supportedCultures);
        });
        // builder.Services.AddEmailSender(builder.Configuration);

        #endregion

        #region 帳號密碼驗證規則配置

        builder.Services.Configure<Matrix.Services.UserValidationOptions>(options =>
        {
            // 用戶名規則
            options.UserName.RequiredLength = 3;
            options.UserName.MaximumLength = 20;
            options.UserName.AllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            
            // 密碼規則
            options.Password.RequiredLength = 8;
            options.Password.MaximumLength = 20;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredUniqueChars = 1;
            
            // Email 規則
            options.Email.MaximumLength = 30;
            options.Email.RequireConfirmedEmail = false;
        });

        #endregion

        #region 取消使用 Identity

        // builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        //     .AddEntityFrameworkStores<ApplicationDbContext>();

        #endregion

        #region JWT 設定

        var jwtKey = builder.Configuration["JWT:Key"];
        var jwtIssuer = builder.Configuration["JWT:Issuer"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("JWT Key or Issuer not configured.");
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        #endregion

        #region SMTP 設定

        // 綁定配置中的 GoogleSMTP 區塊到我們的設定類別 (從 user secrets 讀取)
        builder.Services.Configure<GoogleSmtpDTOs>(builder.Configuration.GetSection("GoogleSMTP"));

        // 註冊我們的郵件服務，讓 Controller 可以使用
        builder.Services.AddTransient<IEmailService, GmailService>();

        #endregion

        // 響應壓縮：排除 HTML 避免解碼錯誤
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            // 明確排除 text/html，只壓縮 API 和靜態資源
            options.MimeTypes = new[]
            {
                "application/json",
                "application/javascript",
                "text/javascript",
                "text/css",
                "text/plain",
                "application/xml",
                "text/xml",
                "image/svg+xml"
            };
        });
        
        builder.Services.AddControllersWithViews(options =>
        {
            // 自訂 ModelBinding 錯誤訊息提供者
            options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "此欄位為必填");
        })
        .AddJsonOptions(options =>
        {
            // 防止 JSON 序列化循環引用
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
        builder.Services.AddRazorPages();

        #region 配置 Anti-forgery 以支援 Ajax 請求

        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "RequestVerificationToken";
        });

        #endregion

        // 動態端口配置
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
                Console.WriteLine($"應用程式將在 {newUrl} 上執行");
            }
            else
            {
                Console.WriteLine($"應用程式將在 {originalUrls} 上執行");
            }
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
            app.UseMigrationsEndPoint();
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseResponseCompression(); // 啟用響應壓縮
        app.UseStaticFiles();
        app.UseRouting();
        app.UseRequestLocalization();

        #region JWT 驗證機制

        app.UseMiddleware<JwtCookieMiddleware>();

        #endregion

        #region Dashboard 權限檢查

        app.UseDashboardAccess();

        #endregion

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers(); // 啟用 API 控制器的屬性路由

        // Areas 路由 (優先處理)
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Overview}/{action=Index}/{id?}");

        // 預設路由
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }

    private static int FindAvailablePort(int startPort)
    {
        int port = startPort;
        while (port <= 65535)
        {
            if (IsPortAvailable(port))
            {
                return port;
            }
            port++;
        }
        throw new InvalidOperationException($"No available port found starting from {startPort}");
    }

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
}
