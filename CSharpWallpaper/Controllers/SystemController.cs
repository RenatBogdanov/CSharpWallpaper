using CSharpWallpaper.Data;
using CSharpWallpaper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace CSharpWallpaper.Controllers
{
    // Этот атрибут говорит, что для внешнего мира контроллер все еще называется Installing
    [Route("Installing")]
    public class SystemController(
    IWallpaperService wallpaperService,
    IFileSyncService fileSyncService) : Controller
    {
        // Этот метод будет открываться и по /Installing, и по /Installing/Installing
        [HttpGet("")]
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
            return View("Installing");
        }


        // Доступны по /Installing/Set, /Installing/FillDb и т.д.
        [HttpPost("Set")]
        public IActionResult SetWallpaper([FromQuery] string imageUrl)
        {
            var success = wallpaperService.SetWallpaper(imageUrl);

            if (success) return Ok(new { success = true });
            return BadRequest("Не удалось установить обои.");
        }

        // Доступно по адресу: /Installing/GetCurrentWallpaperImage
        [SupportedOSPlatform("windows")]
        [HttpGet("GetCurrentWallpaperImage")]
        public IActionResult GetCurrentWallpaperImage()
        {
            string? wallpaperPath = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null) as string;

            if (string.IsNullOrEmpty(wallpaperPath) || !System.IO.File.Exists(wallpaperPath))
                wallpaperPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes\TranscodedWallpaper");

            if (System.IO.File.Exists(wallpaperPath))
                return File(System.IO.File.OpenRead(wallpaperPath), "image/jpeg");

            return NotFound();
        }

        // Доступно по адресу: /Installing/FillDb
        [HttpGet("FillDb")]
        public async Task<IActionResult> FillDb([FromServices] AppDbContext context)
        {
            // Принудительно создаем файл БД и все таблицы, если их еще нет
            context.Database.EnsureCreated();

            var result = await fileSyncService.SyncWallpapers();
            return Content($"✅ Синхронизация завершена!\nДобавлено: {result.Added}\n🗑 Удалено: {result.Deleted}");
        }

    }
}