using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Models;
using CSharpWallpaper.Data;
using System.Linq;
using CSharpWallpaper.ViewModels;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly AppDbContext _context;

        public MainController(ILogger<MainController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("")]
        [Route("/")]
        public IActionResult Main()
        {
            ViewBag.ActivePage = "Main";

            // 1. Получаем все обои из базы
            var allItems = _context.Wallpapers.ToList();

            // 2. Делаем случайную выборку для секции "Популярные"
            // Используем Guid.NewGuid() для перемешивания списка
            var randomWallpapers = allItems
                .OrderBy(w => Guid.NewGuid())
                .Take(8)
                .ToList();

            // 3. Собираем ViewModel
            var mainViewModel = new WallpaperCollectionViewModel
            {
                // Секция 1: 8 случайных карточек (смесь категорий)
                SimpleCards = randomWallpapers.Select(w => new SimpleImageCardViewModel
                {
                    ImageUrl = w.ImageUrl,
                    AltText = w.Title,
                    ClickUrl = "/Installing?imageUrl=" + w.ImageUrl
                }).ToList(),

                // Секция 2: Уникальные категории (папки)
                // Берем по 1 фото из каждой папки, максимум 4 штуки
                ImageTextCards = allItems
                    .GroupBy(w => w.Category)
                    .Select(g => g.First())
                    .Take(4)
                    .Select(w => new ImageTextCardViewModel
                    {
                        ImageUrl = w.ImageUrl,
                        Title = w.Category ?? "Общие",
                        Description = $"Смотреть коллекцию {w.Category}",
                        ClickUrl = $"/Collections?category={w.Category}"
                    }).ToList(),

                IconCards = new List<ImageIconTextCardViewModel>()
            };

            return View(mainViewModel);
        }

<<<<<<< HEAD
        [HttpGet("Privacy")]
        public IActionResult Privacy()
        {
            ViewBag.ActivePage = "Main";
            return View();
        }

=======
        //public IActionResult Privacy() { }
        
>>>>>>> d41afb0b9b800c808fd3db0c41f090684450d2c0
        [HttpGet("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
