using CSharpWallpaper.Interfaces;
using CSharpWallpaper.Data;
using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.IO;

namespace CSharpWallpaper.Controllers
{
    public class InstallingController(IWallpaperService wallpaperService, AppDbContext context) : Controller
    {
        // Метод для "тихого" выбора картинки без перехода
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

            // Если пришли по прямой ссылке — сохраняем через сервис
            if (!string.IsNullOrEmpty(imageUrl))
            {
                wallpaperService.SaveSelectedWallpaper(imageUrl);
                ViewBag.ImageUrl = imageUrl;
            }
            else
            {
                // Берем текущий выбор из куки через сервис
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

            // Формируем полный физический путь к файлу в wwwroot
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", finalUrl.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                wallpaperService.SetWallpaper(fullPath);
                return Ok(new { success = true });
            }
            return NotFound("Файл не найден на сервере");
        }

        [HttpGet("Installing/GetCurrentWallpaperImage")]
        public IActionResult GetCurrentWallpaperImage()
        {
            // Эта логика получения файла из Windows остается здесь или переносится в сервис
            string wallpaperPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null);

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
