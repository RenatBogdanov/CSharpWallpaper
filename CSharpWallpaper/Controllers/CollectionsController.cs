using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class CollectionsController : Controller
    {
        [HttpGet("")] // /collections
        public IActionResult Collections()
        {
            ViewBag.ActivePage = "Collections";
            var collectModel = new WallpaperCollectionViewModel
            {
                // Вариант 1: Простые карточки
                SimpleCards = new List<SimpleImageCardViewModel>
            {
                new SimpleImageCardViewModel
                {
                    ImageUrl = "/images/wallpapers/ireland.jpg",
                    ClickUrl = "/wallpapers/1"
                },
                new SimpleImageCardViewModel
                {
                    ImageUrl = "/images/wallpapers/ireland.jpg",
                    ClickUrl = "/wallpapers/2"
                }
            },

                // Вариант 2: Картинка + текст
                ImageTextCards = new List<ImageTextCardViewModel>
            {
                new ImageTextCardViewModel
                {
                    ImageUrl = "/images/wallpapers/ireland.jpg",
                    AltText = "Природа",
                    Title = "Природа",
                    Description = "Красивые пейзажи и природа",
                    ClickUrl = "/wallpapers/1"
                },
                new ImageTextCardViewModel
                {
                    ImageUrl = "/images/wallpapers/ireland.jpg",
                    AltText = "Техника",
                    Title = "Техника",
                    Description = "Машины, самолеты, корабли",
                    ClickUrl = "/wallpapers/1"
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

            return View(collectModel);
        }
    }
}
