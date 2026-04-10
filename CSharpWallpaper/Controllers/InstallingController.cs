using CSharpWallpaper.Interfaces;
using CSharpWallpaper.Data;
using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.IO;
using System.Runtime.Versioning; // Добавлено для атрибута SupportedOSPlatform

namespace CSharpWallpaper.Controllers
{
    public class InstallingController(IWallpaperService wallpaperService, AppDbContext context) : Controller
    {
        [HttpPost("Installing/Select")]
        public IActionResult Select([FromBody] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return BadRequest();

            wallpaperService.SaveSelectedWallpaper(imageUrl);
            return Ok(new { success = true });
        }

        [HttpGet("Installing")]
        public IActionResult Installing(string imageUrl)
        {
            ViewBag.ActivePage = "Installing";

            if (!string.IsNullOrEmpty(imageUrl))
            {
                wallpaperService.SaveSelectedWallpaper(imageUrl);
                ViewBag.ImageUrl = imageUrl;
            }
            else
            {
                ViewBag.ImageUrl = wallpaperService.GetSelectedWallpaper();
            }

            ViewBag.CurrentWallpaperPath = wallpaperService.GetCurrentWallpaperPath();
            return View();
        }

        [HttpPost("Installing/Set")]
        public IActionResult SetWallpaper(string imageUrl)
        {
            var finalUrl = imageUrl ?? wallpaperService.GetSelectedWallpaper();
            if (string.IsNullOrEmpty(finalUrl)) return BadRequest("Картинка не выбрана");

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", finalUrl.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                wallpaperService.SetWallpaper(fullPath);
                return Ok(new { success = true });
            }
            return NotFound("Файл не найден на сервере");
        }

        [SupportedOSPlatform("windows")] // Исправляет предупреждение CA1416
        [HttpGet("Installing/GetCurrentWallpaperImage")]
        public IActionResult GetCurrentWallpaperImage()
        {
            // Используем 'as string', чтобы убрать предупреждение CS8600 (nullable)
            string? wallpaperPath = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null) as string;

            if (string.IsNullOrEmpty(wallpaperPath) || !System.IO.File.Exists(wallpaperPath))
                wallpaperPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes\TranscodedWallpaper");

            if (System.IO.File.Exists(wallpaperPath))
                return File(System.IO.File.OpenRead(wallpaperPath), "image/jpeg");

            return NotFound();
        }

        [HttpGet("Installing/FillDb")]
        public IActionResult FillDb()
        {
            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "wallpapers");
            if (!Directory.Exists(rootPath)) return Content("Папка не найдена");

            int addedCount = 0;
            var directories = Directory.GetDirectories(rootPath);

            foreach (var dir in directories)
            {
                string categoryName = Path.GetFileName(dir);
                foreach (var filePath in Directory.GetFiles(dir))
                {
                    var fileName = Path.GetFileName(filePath);
                    var webPath = $"/images/wallpapers/{categoryName}/{fileName}";

                    if (!context.Wallpapers.Any(w => w.ImageUrl == webPath))
                    {
                        context.Wallpapers.Add(new Wallpaper
                        {
                            Title = fileName,
                            ImageUrl = webPath,
                            Category = categoryName,
                            IsPopular = addedCount < 5
                        });
                        addedCount++;
                    }
                }
            }
            context.SaveChanges();
            return Content($"Добавлено: {addedCount}");
        }
    }
}
