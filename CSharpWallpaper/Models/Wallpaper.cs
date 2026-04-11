namespace CSharpWallpaper.Models
{
    public class Wallpaper
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public bool IsPopular { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }



}
