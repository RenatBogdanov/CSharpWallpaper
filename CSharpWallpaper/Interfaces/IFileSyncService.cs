using System.Threading.Tasks;

namespace CSharpWallpaper.Interfaces
{
    public interface IFileSyncService
    {
        // Возвращает кортеж с количеством добавленных и удаленных обоев
        Task<(int Added, int Deleted)> SyncWallpapersAsync();
    }
}