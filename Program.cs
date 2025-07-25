// using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Matrix.Data;
using Matrix.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Matrix;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        // ------------------ 註冊 Service ------------------

        builder.Services.AddScoped<Matrix.Services.Interfaces.IUserService, UserService>();
        builder.Services.AddScoped<ArticleService>();
        builder.Services.AddScoped<NotificationService>();

        // -------------------------------------------------

        // ---------------- 取消使用 Identity ---------------

        // builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        //     .AddEntityFrameworkStores<ApplicationDbContext>();

        // -------------------------------------------------

        builder.Services.AddControllersWithViews()
            .AddViewLocalization(); // 啟用視圖本地化
        builder.Services.AddRazorPages();

        // 配置 Anti-forgery 以支援 Ajax 請求
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "RequestVerificationToken";
        });

        // -------------------- 本地化設定 --------------------
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

        // -------------------------------------------------

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

        // -------------------- 本地化設定 --------------------
        var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(localizationOptions.Value);
        // -------------------------------------------------

        app.UseRouting();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}