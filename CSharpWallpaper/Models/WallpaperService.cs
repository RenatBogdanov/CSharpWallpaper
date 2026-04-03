using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace CSharpWallpaper.Services
{
    public class WallpaperService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        public void SetWallpaper(string path)
        {
            // Важно: путь должен быть абсолютным
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        public string GetCurrentWallpaperPath()
        {
            // Читаем путь из реестра пользователя
            string path = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", null);
            return path;
        }
    }
}
