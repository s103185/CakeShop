using CakeShop.Data;
using CakeShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. 設定 SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. 設定 Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. 註冊自定義的 Email 服務
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- 新增：自動執行資料庫遷移 (Migration) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // 這行指令會自動檢查資料庫並建立遺失的資料表（如 Cakes）
        context.Database.Migrate();
        Console.WriteLine("資料庫遷移成功！");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "在執行資料庫遷移時發生錯誤。");
    }
}
// ------------------------------------------

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // 啟用身分驗證
app.UseAuthorization();  // 啟用授權

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity UI 需要

app.Run();