using System.Diagnostics;
using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        [Route("/")]
        public IActionResult Main()
        {
            ViewBag.ActivePage = "Main";
            var mainViewModel = new WallpaperCollectionViewModel
            {
                // Вариант 1: Простые карточки
                SimpleCards = new List<SimpleImageCardViewModel>
            {
                new SimpleImageCardViewModel
                {
                    ImageUrl = "/images/wallpapers/japan.jpg",
                    ClickUrl = "/wallpapers/1"
                },
                new SimpleImageCardViewModel
                {
                    ImageUrl = "/images/wallpapers/japan.jpg",
                    ClickUrl = "/wallpapers/2"
                }
            },
                // TODO: Добавить переход по ссылке этому блоку
                // Вариант 2: Картинка + текст
                ImageTextCards = new List<ImageTextCardViewModel>
            {
                new ImageTextCardViewModel
                {
                    ImageUrl = "/images/wallpapers/japan.jpg",
                    AltText = "Природа",
                    Title = "Природа",
                    Description = "Красивые пейзажи и природа",
                    ClickUrl = "/wallpapers/1"
                },
                new ImageTextCardViewModel
                {
                    ImageUrl = "/images/wallpapers/japan.jpg",
                    AltText = "Техника",
                    Title = "Техника",
                    Description = "Машины, самолеты, корабли",
                    ClickUrl = "/wallpapers/2"
                }
            },

                // Вариант 3: Картинка + иконка + текст (как Ирландия)
                IconCards = new List<ImageIconTextCardViewModel>
            {
                new ImageIconTextCardViewModel
                {
                    ImageUrl = "/images/wallpapers/ireland.jpg",
                    AltText = "Ирландия",
                    IconUrl = "/images/flags/flagIreland.jpg", // или "/icons/ireland.svg"
                    Title = "Ирландия",
                    Description = "Красивые пейзажи Ирландии",
                    ButtonText = "Смотреть коллекцию",
                    ButtonUrl = "/collections/ireland"
                },
                new ImageIconTextCardViewModel
                {
                    ImageUrl = "/images/wallpapers/scotland.jpg",
                    AltText = "Шотландия",
                    IconUrl = "/images/flags/flagScotland.png",
                    Title = "Шотландия",
                    Description = "Горы и озера Шотландии",
                    ButtonText = "Смотреть коллекцию",
                    ButtonUrl = "/collections/scotland"
                }
            }
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
