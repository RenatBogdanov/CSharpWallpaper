using CSharpWallpaper.Data;
using CSharpWallpaper.Interfaces;
using CSharpWallpaper.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace CSharpWallpaper.Services
{
    public class WallpaperService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env) : IWallpaperService
    {
        public async Task<WallpaperCollectionViewModel> GetMainPageModelAsync()
        {
            // Секция 1: 12 случайных карточек (работает отлично)
            var randomCards = await context.Wallpapers
                .OrderBy(w => EF.Functions.Random())
                .Take(12)
                .Select(w => new SimpleImageCardViewModel
                {
                    ImageUrl = w.ImageUrl,
                    AltText = w.Title,
                    ClickUrl = "/Installing?imageUrl=" + w.ImageUrl
                })
                .ToListAsync();

            // Секция 2: Уникальные категории ОДНИМ запросом к БД
            // Группируем по категории и сразу берем первую картинку для каждой группы прямо в SQL
            var categoryData = await context.Wallpapers
                .GroupBy(w => w.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    ImageUrl = g.Select(w => w.ImageUrl).FirstOrDefault()
                })
                .OrderBy(c => EF.Functions.Random()) // Рандомизируем уже сгруппированные категории
                .Take(4)
                .ToListAsync();

            // Формируем ViewModel в памяти без лишних обращений к context
            var imageTextCards = categoryData.Select(c => new ImageTextCardViewModel
            {
                ImageUrl = c.ImageUrl,
                Title = c.Category ?? "Общие",
                Description = $"Смотреть коллекцию {c.Category}",
                ClickUrl = $"/Collections?category={c.Category}"
            }).ToList();

            return new WallpaperCollectionViewModel
            {
                SimpleCards = randomCards,
                ImageTextCards = imageTextCards
            };
        }

        public async Task<WallpaperCollectionViewModel> GetCategoriesModelAsync()
        {
            // Точно так же оптимизируем страницу всех категорий
            var categoryData = await context.Wallpapers
                .GroupBy(w => w.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    ImageUrl = g.Select(w => w.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            var imageTextCards = categoryData.Select(c => new ImageTextCardViewModel
            {
                ImageUrl = c.ImageUrl,
                Title = c.Category ?? "Общие",
                Description = $"Смотреть обои из папки {c.Category}",
                ClickUrl = $"/Collections?category={c.Category}"
            }).ToList();

            return new WallpaperCollectionViewModel
            {
                ImageTextCards = imageTextCards
            };
        }


        public async Task<WallpaperCollectionViewModel> GetCategoryItemsModelAsync(string category)
        {
            var cards = await context.Wallpapers
                .Where(w => w.Category == category)
                .Select(w => new SimpleImageCardViewModel
                {
                    ImageUrl = w.ImageUrl,
                    AltText = w.Title,
                    ClickUrl = "#"
                })
                .ToListAsync();

            return new WallpaperCollectionViewModel
            {
                SimpleCards = cards
            };
        }

        public void SaveSelectedWallpaper(string imageUrl)
        {
            var options = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
            httpContextAccessor.HttpContext?.Response.Cookies.Append("LastSelectedWallpaper", imageUrl, options);
        }

        public string GetSelectedWallpaper()
        {
            return httpContextAccessor.HttpContext?.Request.Cookies["LastSelectedWallpaper"] ?? "";
        }

        [SupportedOSPlatform("windows")]
        public string GetCurrentWallpaperPath()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "";
            return Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null) as string ?? "";
        }

        public bool SetWallpaper(string imageUrl)
        {
            var finalUrl = imageUrl ?? GetSelectedWallpaper();
            if (string.IsNullOrEmpty(finalUrl)) return false;

            // Заменяем все веб-слеши (/) на системные (в Windows это \)
            // Убираем первый слеш, чтобы Path.Combine не воспринял это как корень диска
            string normalizedPath = finalUrl.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            string fullPath = Path.GetFullPath(Path.Combine(env.WebRootPath, normalizedPath));

            if (File.Exists(fullPath))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    SetWindowsWallpaper(fullPath);
                    return true;
                }
            }
            return false;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [SupportedOSPlatform("windows")]
        private void SetWindowsWallpaper(string fullPath)
        {
            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, fullPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}