namespace CSharpWallpaper.Models
{
    public class Wallpaper
    {
        public int Id { get; set; }
        public string Title { get; set; }        // Название файла
        public string ImageUrl { get; set; }     // Путь для сайта: /images/wallpapers/1.jpg
        public string Category { get; set; }     // Категория (опционально)
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }


}
