namespace CSharpWallpaper.Models
{
    public class Wallpaper
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; } // Это и будет наша коллекция (Природа, Машины и т.д.)
        public bool IsPopular { get; set; }  // Для секции "Популярные"
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }



}
