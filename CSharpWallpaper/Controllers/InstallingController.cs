using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Services;
using Microsoft.Win32;
using System.IO;
using CSharpWallpaper.Data;
using CSharpWallpaper.Models;

namespace CSharpWallpaper.Controllers
{
    public class InstallingController : Controller
    {
        private readonly WallpaperService _wallpaperService = new WallpaperService();
        private readonly AppDbContext _context;

        public InstallingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Installing")]
        public IActionResult Installing(string imageUrl)
        {
            // Для Дани (подсветка меню)
            ViewBag.ActivePage = "Installing";

            ViewBag.ImageUrl = imageUrl;

            // Получаем путь к текущим обоям через сервис
            var currentPath = _wallpaperService.GetCurrentWallpaperPath();
            ViewBag.CurrentWallpaperPath = currentPath;

            return View();
        }

        [HttpPost("Installing/Set")]
        public IActionResult SetWallpaper(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return BadRequest("URL не указан");

            // Превращаем относительный путь (/images/...) в полный физический путь на диске
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                _wallpaperService.SetWallpaper(fullPath);
                return Ok(new { success = true });
            }

            return NotFound($"Файл не найден по пути: {fullPath}");
        }

        [HttpGet("Installing/GetCurrentWallpaperImage")]
        public IActionResult GetCurrentWallpaperImage()
        {
            // Пробуем взять путь из реестра
            string wallpaperPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null);

            // Если там пусто или файл перемещен, берем из системного кэша Windows
            if (string.IsNullOrEmpty(wallpaperPath) || !System.IO.File.Exists(wallpaperPath))
            {
                wallpaperPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Themes\TranscodedWallpaper");
            }

            if (System.IO.File.Exists(wallpaperPath))
            {
                var image = System.IO.File.OpenRead(wallpaperPath);
                return File(image, "image/jpeg");
            }

            return NotFound();
        }

        [HttpGet("Installing/FillDb")]
        public IActionResult FillDb()
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "wallpapers");

            if (!Directory.Exists(folderPath))
            {
                return Content($"Папка не найдена: {folderPath}");
            }

            var files = Directory.GetFiles(folderPath);
            int addedCount = 0;

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var ext = Path.GetExtension(fileName).ToLower();

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    if (!_context.Wallpapers.Any(w => w.Title == fileName))
                    {
                        _context.Wallpapers.Add(new Wallpaper
                        {
                            Title = fileName,
                            ImageUrl = "/images/wallpapers/" + fileName,
                            Category = "Общие"
                        });
                        addedCount++;
                    }
                }
            }

            _context.SaveChanges();
            return Content($"База синхронизирована! Добавлено новых обоев: {addedCount}");
        }
    }
}
