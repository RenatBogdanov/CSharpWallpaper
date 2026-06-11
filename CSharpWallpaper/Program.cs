using Microsoft.EntityFrameworkCore;
using CSharpWallpaper.Data;
using CSharpWallpaper.Interfaces;
using CSharpWallpaper.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем стандартные сервисы MVC
builder.Services.AddControllersWithViews();

// 2. Настраиваем базу данных SQLite жестко рядом с .exe
// (Иначе при запуске через ярлык база создастся в другой папке и будет пустой)
var exePath = AppDomain.CurrentDomain.BaseDirectory;
var dbPath = Path.Combine(exePath, "app.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));


// 3. Добавляем доступ к контексту HTTP. 
// Без этого WallpaperService не сможет работать с Cookies и будет выдавать ошибку при запуске.
builder.Services.AddHttpContextAccessor();

// 4. Регистрируем сервис: связываем интерфейс с его реализацией.
// Это позволит контроллерам запрашивать IWallpaperService в конструкторе.
builder.Services.AddScoped<IWallpaperService, WallpaperService>();

builder.Services.AddScoped<IFileSyncService, FileSyncService>();

// 5. Фоновый сервис, который выполнит синхронизацию при самом первом старте
builder.Services.AddHostedService<DatabaseInitializerService>();

var app = builder.Build();

// Гарантируем, что файл БД и структура таблиц созданы при старте (это работает мгновенно)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Настройка конвейера обработки HTTP-запросов (Middleware)
if (!app.Environment.IsDevelopment())
{
    // Обработка ошибок в рабочем режиме
    app.UseExceptionHandler("/Main/Error");
    app.UseHsts();
}
else
{
    // Оставляем HTTPS-редирект только для разработки в Visual Studio,
    // чтобы в билде .exe он не ломал порты и опрос закрытия вкладки
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --- НАЧАЛО БЛОКА ОПРОСА ДЛЯ ЗАКРЫТИЯ ПРИЛОЖЕНИЯ ---
DateTime lastHeartbeat = DateTime.UtcNow;
bool isMonitoring = false;

// Эндпоинт, который принимает сигналы активности от вкладки браузера
app.MapPost("/api/heartbeat", (IHostApplicationLifetime lifetime) =>
{
    lastHeartbeat = DateTime.UtcNow;

    if (!isMonitoring)
    {
        isMonitoring = true;
        // Фоновый поток мониторинга активности
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(4000); // Проверка каждые 4 секунды

                // Если сигналов от JS не было дольше 6 секунд — корректно тушим .exe
                if ((DateTime.UtcNow - lastHeartbeat).TotalSeconds > 6)
                {
                    lifetime.StopApplication();
                    break;
                }
            }
        });
    }
    return Results.Ok();
});
// --- КОНЕЦ БЛОКА ОПРОСА ДЛЯ ЗАКРЫТИЯ ПРИЛОЖЕНИЯ ---

// Настройка маршрута по умолчанию: Контроллер Main, Метод Main
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Main}/{id?}");

// --- УМНОЕ АВТОМАТИЧЕСКОЕ ОТКРЫТИЕ БРАУЗЕРА (БЕЗ ХАРДКОДА ПОРТА) ---
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        // Динамически считываем адрес и порт, который Kestrel выбрал для запуска
        var server = app.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        var runningUrl = addressesFeature?.Addresses?.FirstOrDefault();

        // Если порт успешно определен — открываем вкладку браузера на этом адресе
        if (!string.IsNullOrEmpty(runningUrl))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = runningUrl,
                UseShellExecute = true
            });
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Не удалось открыть браузер: {ex.Message}");
    }
});

app.Run();

// --- КЛАСС ФОНОВОГО СЕРВИСА ДЛЯ ИНИЦИАЛИЗАЦИИ БД ---
// (Он запускается средой выполнения автоматически сразу ПОСЛЕ старта Kestrel)
public class DatabaseInitializerService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Проверяем, пустая ли таблица Wallpapers
        if (!await context.Wallpapers.AnyAsync(cancellationToken))
        {
            var fileSyncService = scope.ServiceProvider.GetRequiredService<IFileSyncService>();
            // Безопасно вызываем ваш метод синхронизации в изолированном контексте
            await fileSyncService.SyncWallpapers();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
