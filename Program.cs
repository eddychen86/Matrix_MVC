using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using Matrix.Middleware;
using Matrix.Services.Extensions;
using DotNetEnv;
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

        // 載入 .env
        DotNetEnv.Env.Load();

        var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var dbAccount = Environment.GetEnvironmentVariable("DB_ACCOUNT");
        var dbPwd = Environment.GetEnvironmentVariable("DB_PWD");

        var connectionString = $"Server={dbServer};Database={dbName};User Id={dbAccount};Password={dbPwd};MultipleActiveResultSets=true";

        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

        // Add services to the container.
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseLazyLoadingProxies().UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        #region 註冊 Service

        builder.Services.AddScoped<Matrix.Services.Interfaces.IUserService, UserService>();
        builder.Services.AddScoped<ArticleService>();
        builder.Services.AddScoped<NotificationService>();

        builder.Services.AddEmailSender(builder.Configuration);

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

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

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

        builder.Services.AddControllersWithViews()
            .AddViewLocalization(); // 啟用視圖本地化
        builder.Services.AddRazorPages();

        // 配置 Anti-forgery 以支援 Ajax 請求
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "RequestVerificationToken";
        });

        #region 多國語系設定 (Part 1/2)
        
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("zh-TW")
            };

            options.DefaultRequestCulture = new RequestCulture("zh-TW");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            
            // 配置本地化提供者順序：Query String > Cookie > Accept-Language > 預設
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };
            // 當所有提供者都沒有結果時，使用預設文化 zh-TW
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
        app.UseStaticFiles();

        #region 多國語系設定 (Part 2/2)

        var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(localizationOptions.Value);

        #endregion------------------------

        app.UseRouting();

        #region JWT 驗證機制

        app.UseMiddleware<JwtCookieMiddleware>();

        #endregion

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}