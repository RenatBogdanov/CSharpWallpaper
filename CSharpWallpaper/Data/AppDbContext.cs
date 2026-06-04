using Microsoft.EntityFrameworkCore;
using CSharpWallpaper.Models;

namespace CSharpWallpaper.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Wallpaper> Wallpapers { get; set; }
    }
}
