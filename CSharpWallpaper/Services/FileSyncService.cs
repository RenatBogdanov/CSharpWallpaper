using CSharpWallpaper.Data;
using CSharpWallpaper.Interfaces;
using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpWallpaper.Services
{
    public class FileSyncService(AppDbContext context, IWebHostEnvironment env) : IFileSyncService
    {
        public async Task<(int Added, int Deleted)> SyncWallpapers()
        {
            // Используем IWebHostEnvironment для правильного пути к wwwroot
            string rootPath = Path.Combine(env.WebRootPath, "images", "wallpapers");
            if (!Directory.Exists(rootPath)) return (0, 0);

            var diskPaths = new HashSet<string>();
            var allDbWallpapers = await context.Wallpapers.ToListAsync();

            int addedCount = 0;
            int deletedCount = 0;

            foreach (var dir in Directory.GetDirectories(rootPath))
            {
                string categoryName = Path.GetFileName(dir);
                foreach (var filePath in Directory.GetFiles(dir))
                {
                    var fileName = Path.GetFileName(filePath);
                    // Веб-пути всегда должны использовать прямой слеш '/'
                    var webPath = $"/images/wallpapers/{categoryName}/{fileName}";
                    diskPaths.Add(webPath);

                    // Добавляем новые картинки, которых ещё нет в БД
                    if (!allDbWallpapers.Any(w => w.ImageUrl == webPath))
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

            // Удаляем из БД те записи, файлов которых больше нет на диске
            var toDelete = allDbWallpapers.Where(w => !diskPaths.Contains(w.ImageUrl)).ToList();
            if (toDelete.Any())
            {
                context.Wallpapers.RemoveRange(toDelete);
                deletedCount = toDelete.Count;
            }

            await context.SaveChangesAsync();
            return (addedCount, deletedCount);
        }
    }
}