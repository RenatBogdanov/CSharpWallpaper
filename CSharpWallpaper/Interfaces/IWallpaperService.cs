using CSharpWallpaper.ViewModels;
using System.Threading.Tasks;

namespace CSharpWallpaper.Interfaces
{
    public interface IWallpaperService
    {
        Task<WallpaperCollectionViewModel> GetMainPageModelAsync();
        Task<WallpaperCollectionViewModel> GetCategoriesModelAsync();
        Task<WallpaperCollectionViewModel> GetCategoryItemsModelAsync(string category);
        void SaveSelectedWallpaper(string imageUrl);
        string GetSelectedWallpaper();
        string GetCurrentWallpaperPath();
        bool SetWallpaper(string imageUrl);
    }
}