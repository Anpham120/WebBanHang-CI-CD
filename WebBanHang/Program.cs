using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDirectory);
var dbPath = Path.Combine(dataDirectory, "webbanhang.db");
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider.GetRequiredService<ShopDbContext>());
}

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "local"
}));
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
