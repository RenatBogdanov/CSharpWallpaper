using Microsoft.EntityFrameworkCore;
using CSharpWallpaper.Data;
using CSharpWallpaper.Interfaces;
using CSharpWallpaper.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем стандартные сервисы MVC
builder.Services.AddControllersWithViews();

// 2. Настраиваем базу данных SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


// 3. Добавляем доступ к контексту HTTP. 
// Без этого WallpaperService не сможет работать с Cookies и будет выдавать ошибку при запуске.
builder.Services.AddHttpContextAccessor();

// 4. Регистрируем сервис: связываем интерфейс с его реализацией.
// Это позволит контроллерам запрашивать IWallpaperService в конструкторе.
builder.Services.AddScoped<IWallpaperService, WallpaperService>();

builder.Services.AddScoped<IFileSyncService, FileSyncService>();

var app = builder.Build();

// Настройка конвейера обработки HTTP-запросов (Middleware)
if (!app.Environment.IsDevelopment())
{
    // Обработка ошибок в рабочем режиме
    app.UseExceptionHandler("/Main/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Настройка маршрута по умолчанию: Контроллер Main, Метод Main
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Main}/{id?}");

app.Run();
