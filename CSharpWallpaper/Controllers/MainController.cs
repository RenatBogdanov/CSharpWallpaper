using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.ViewModels;
using CSharpWallpaper.Interfaces;
using System.Threading.Tasks;
using CSharpWallpaper.Models;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class MainController(IWallpaperService wallpaperService) : Controller
    {
        [HttpGet("")]
        [Route("/")]
        public async Task<IActionResult> Main()
        {
            ViewBag.ActivePage = "Main";
            var model = await wallpaperService.GetMainPageModelAsync();
            return View(model);
        }

        [HttpGet("Privacy")]
        public IActionResult Privacy()
        {
            ViewBag.ActivePage = "Main";
            return View();
        }

        [HttpGet("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}