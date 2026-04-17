using System;

namespace CSharpWallpaper.Models
{
    public class Wallpaper
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}