using CSharpWallpaper.Data;
using CSharpWallpaper.Interfaces;
using CSharpWallpaper.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CSharpWallpaper.Services
{
    public class WallpaperService(AppDbContext context, IHttpContextAccessor httpContextAccessor) : IWallpaperService
    {
        public WallpaperCollectionViewModel GetMainPageModel()
        {
            var allItems = context.Wallpapers.ToList();

            return new WallpaperCollectionViewModel
            {
                // Секция 1: 12 случайных карточек для "Популярного"
                SimpleCards = allItems
                    .OrderBy(w => Guid.NewGuid())
                    .Take(12)
                    .Select(w => new SimpleImageCardViewModel
                    {
                        ImageUrl = w.ImageUrl,
                        AltText = w.Title,
                        ClickUrl = "/Installing?imageUrl=" + w.ImageUrl
                    }).ToList(),

                // Секция 2: Уникальные категории (макс 4)
                ImageTextCards = allItems
                    .GroupBy(w => w.Category)
                    .Select(g => g.First())
                    .Take(4)
                    .Select(w => new ImageTextCardViewModel
                    {
                        ImageUrl = w.ImageUrl,
                        Title = w.Category ?? "Общие",
                        Description = $"Смотреть коллекцию {w.Category}",
                        ClickUrl = $"/Collections?category={w.Category}"
                    }).ToList()
            };
        }

        public WallpaperCollectionViewModel GetCategoriesModel()
        {
            var dbItems = context.Wallpapers.ToList();
            return new WallpaperCollectionViewModel
            {
                ImageTextCards = dbItems
                    .GroupBy(w => w.Category)
                    .Select(g => g.First())
                    .Select(w => new ImageTextCardViewModel
                    {
                        ImageUrl = w.ImageUrl,
                        Title = w.Category ?? "Общие",
                        Description = $"Смотреть обои из папки {w.Category}",
                        ClickUrl = $"/Collections?category={w.Category}"
                    }).ToList()
            };
        }

        public WallpaperCollectionViewModel GetCategoryItemsModel(string category)
        {
            return new WallpaperCollectionViewModel
            {
                SimpleCards = context.Wallpapers
                    .Where(w => w.Category == category)
                    .Select(w => new SimpleImageCardViewModel
                    {
                        ImageUrl = w.ImageUrl,
                        AltText = w.Title,
                        ClickUrl = "#"
                    }).ToList()
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
            return Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null) as string ?? "";
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [SupportedOSPlatform("windows")]
        public void SetWallpaper(string fullPath)
        {
            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, fullPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
