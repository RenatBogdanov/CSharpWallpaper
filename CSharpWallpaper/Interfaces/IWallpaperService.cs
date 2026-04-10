using CSharpWallpaper.ViewModels;

namespace CSharpWallpaper.Interfaces
{
    public interface IWallpaperService
    {
        WallpaperCollectionViewModel GetMainPageModel();
        WallpaperCollectionViewModel GetCategoriesModel();
        WallpaperCollectionViewModel GetCategoryItemsModel(string category);
        void SaveSelectedWallpaper(string imageUrl);
        string GetSelectedWallpaper();
        string GetCurrentWallpaperPath();
        void SetWallpaper(string imageUrl);
    }
}
