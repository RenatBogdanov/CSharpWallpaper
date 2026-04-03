using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Services;
using Microsoft.Win32;
using System.IO;
using CSharpWallpaper.Data;
using CSharpWallpaper.Models; // Замени на твой namespace, где лежит модель Wallpaper

namespace CSharpWallpaper.Controllers
{
    public class InstallingController : Controller
    {
        private readonly WallpaperService _wallpaperService = new WallpaperService();
        private readonly AppDbContext _context;

        // Внедряем контекст БД через конструктор
        public InstallingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Installing")]
        public IActionResult Installing(string imageUrl)
        {
            ViewBag.ImageUrl = imageUrl;
            var currentPath = _wallpaperService.GetCurrentWallpaperPath();
            ViewBag.CurrentWallpaperPath = currentPath;

            return View();
        }

        [HttpPost("Installing/Set")]
        public IActionResult SetWallpaper(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return BadRequest();

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                _wallpaperService.SetWallpaper(fullPath);
                return Ok(new { success = true });
            }

            return NotFound("Файл не найден");
        }

        [HttpGet("Installing/GetCurrentWallpaperImage")]
        public IActionResult GetCurrentWallpaperImage()
        {
            string wallpaperPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null);

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

        // МЕТОД ДЛЯ ЗАПОЛНЕНИЯ БД ИЗ ПАПКИ
        [HttpGet("Installing/FillDb")]
        public IActionResult FillDb()
        {
            // Путь к папке с обоями в wwwroot
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "wallpapers");

            if (!Directory.Exists(folderPath))
            {
                return Content($"Папка не найдена по пути: {folderPath}");
            }

            var files = Directory.GetFiles(folderPath);
            int addedCount = 0;

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                // Проверяем только картинки (jpg, png, jpeg)
                var ext = Path.GetExtension(fileName).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    // Проверяем, нет ли уже такой картинки в базе
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
            return Content($"Готово! Добавлено новых записей: {addedCount}");
        }
    }
}
