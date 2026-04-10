namespace CSharpWallpaper.ViewModels
{
    // Вариант 1: Только картинка
    public class SimpleImageCardViewModel
    {
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public string ClickUrl { get; set; } // опционально
    }

    // Вариант 2: Картинка + текст
    public class ImageTextCardViewModel
    {
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ClickUrl { get; set; } // опционально
    }

    // Вариант 3: Картинка + иконка + текст (как на фото)
    public class ImageIconTextCardViewModel
    {
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public string IconUrl { get; set; } // или IconCssClass для FontAwesome
        public string Title { get; set; }
        public string Description { get; set; }
        public string ButtonText { get; set; }
        public string ButtonUrl { get; set; }
    }

    // Основная ViewModel для страницы
    public class WallpaperCollectionViewModel
    {
        public List<SimpleImageCardViewModel> SimpleCards { get; set; }
        public List<ImageTextCardViewModel> ImageTextCards { get; set; }
        public List<ImageIconTextCardViewModel> IconCards { get; set; }
    }
}
