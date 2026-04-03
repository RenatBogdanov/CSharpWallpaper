using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Models;
using CSharpWallpaper.Data;
using System.Linq;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly AppDbContext _context;

        // Внедряем логгер (от Дани) и контекст БД (от Рената)
        public MainController(ILogger<MainController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("")]
        [Route("/")]
        public IActionResult Main()
        {
            // Устанавливаем активную страницу для меню (нужно Дане)
            ViewBag.ActivePage = "Main";

            // Получаем все записи из базы данных SQLite
            var dbItems = _context.Wallpapers.ToList();

            // Формируем модель, которую ожидает View
            var mainViewModel = new WallpaperCollectionViewModel
            {
                // Секция 1: Простые карточки (берем все из базы)
                SimpleCards = dbItems.Select(w => new SimpleImageCardViewModel
                {
                    ImageUrl = w.ImageUrl,
                    AltText = w.Title,
                    ClickUrl = "/Installing?imageUrl=" + w.ImageUrl
                }).ToList(),

                // Секция 2: Картинка + текст (Категории)
                // Для примера берем те же данные, Даня потом стилизует
                ImageTextCards = dbItems.Take(2).Select(w => new ImageTextCardViewModel
                {
                    ImageUrl = w.ImageUrl,
                    AltText = w.Title,
                    Title = w.Category ?? "Категория",
                    Description = "Описание из базы",
                    ClickUrl = "/Installing?imageUrl=" + w.ImageUrl
                }).ToList(),

                // Секция 3: Картинка + иконка + текст (Страны)
                IconCards = new List<ImageIconTextCardViewModel>()
                // Сюда можно добавить статику или выборку последних записей
            };

            return View(mainViewModel);
        }

        [HttpGet("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
