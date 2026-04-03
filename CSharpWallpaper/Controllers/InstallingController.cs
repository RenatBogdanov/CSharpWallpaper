using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Services;
using Microsoft.Win32;
using System.IO;
using CSharpWallpaper.Data;
using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Http;

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

        // Метод для "тихого" выбора картинки без перехода
        [HttpPost("Installing/Select")]
        public IActionResult Select([FromBody] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return BadRequest();

            CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
            Response.Cookies.Append("LastSelectedWallpaper", imageUrl, option);
            return Ok(new { success = true });
        }

        [HttpGet("Installing")]
        public IActionResult Installing(string imageUrl)
        {
            ViewBag.ActivePage = "Installing";

            // Если пришли по прямой ссылке с параметром — сохраняем
            if (!string.IsNullOrEmpty(imageUrl))
            {
                CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
                Response.Cookies.Append("LastSelectedWallpaper", imageUrl, option);
                ViewBag.ImageUrl = imageUrl;
            }
            else
            {
                // Иначе берем то, что "накликали" на главной
                ViewBag.ImageUrl = Request.Cookies["LastSelectedWallpaper"];
            }

            ViewBag.CurrentWallpaperPath = _wallpaperService.GetCurrentWallpaperPath();
            return View();
        }

        // Остальные методы (SetWallpaper, GetCurrentWallpaperImage, FillDb) остаются без изменений
        [HttpPost("Installing/Set")]
        public IActionResult SetWallpaper(string imageUrl)
        {
            var finalUrl = imageUrl ?? Request.Cookies["LastSelectedWallpaper"];
            if (string.IsNullOrEmpty(finalUrl)) return BadRequest("Картинка не выбрана");
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", finalUrl.TrimStart('/'));
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
                wallpaperPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes\TranscodedWallpaper");

            if (System.IO.File.Exists(wallpaperPath)) return File(System.IO.File.OpenRead(wallpaperPath), "image/jpeg");
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
                    if (!_context.Wallpapers.Any(w => w.ImageUrl == webPath))
                    {
                        _context.Wallpapers.Add(new Wallpaper { Title = fileName, ImageUrl = webPath, Category = categoryName, IsPopular = addedCount < 5 });
                        addedCount++;
                    }
                }
            }
            _context.SaveChanges();
            return Content($"Добавлено: {addedCount}");
        }
    }
}
