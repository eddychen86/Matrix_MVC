using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Matrix.Middleware;
using Matrix.Services;
using Matrix.Controllers;
// using Microsoft.AspNetCore.Identity;

namespace Matrix;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // 開發環境提示
        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine("💡 如遇 403 錯誤，通常是 port 衝突 - 使用 port 5002 避免 AirTunes");
        }

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
            options.UseLazyLoadingProxies().UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

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

        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ICollectService, CollectService>();
        builder.Services.AddScoped<ArticleService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<Matrix.Controllers.AuthController>();
        builder.Services.AddHttpContextAccessor(); // 為 CustomLocalizer 提供 HttpContext 訪問
        builder.Services.AddScoped<ICustomLocalizer, CustomLocalizer>();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        
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

        // 添加響應壓縮以加速數據傳輸
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "text/plain",
                "text/css",
                "application/javascript",
                "text/html",
                "application/xml",
                "text/xml",
                "application/json; charset=utf-8"
            });
        });
        
        builder.Services.AddControllersWithViews(options =>
        {
            // 自訂 ModelBinding 錯誤訊息提供者
            options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "此欄位為必填");
        });
        builder.Services.AddRazorPages();

        #region 配置 Anti-forgery 以支援 Ajax 請求

        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "RequestVerificationToken";
        });

        #endregion

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
}