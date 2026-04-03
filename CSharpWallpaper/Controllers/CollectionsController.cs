using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Data;
using System.Linq;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class CollectionsController : Controller
    {
        private readonly AppDbContext _context;

        // Внедряем контекст БД
        public CollectionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")] // /collections
        public IActionResult Collections()
        {
            // Устанавливаем активную страницу для навигации Дани
            ViewBag.ActivePage = "Collections";

            // Получаем все обои из базы
            var dbItems = _context.Wallpapers.ToList();

            var collectModel = new WallpaperCollectionViewModel
            {
                // Секция 1: Простые карточки (берем первые 5 для примера)
                SimpleCards = dbItems.Take(5).Select(w => new SimpleImageCardViewModel
                {
                    ImageUrl = w.ImageUrl,
                    AltText = w.Title,
                    ClickUrl = "/Installing?imageUrl=" + w.ImageUrl
                }).ToList(),

                // Секция 2: Категории (выводим по одной картинке из каждой категории)
                ImageTextCards = dbItems
                    .GroupBy(w => w.Category)
                    .Select(g => g.First())
                    .Select(w => new ImageTextCardViewModel
                    {
                        ImageUrl = w.ImageUrl,
                        AltText = w.Category,
                        Title = w.Category ?? "Общие",
                        Description = $"Коллекция обоев: {w.Category}",
                        ClickUrl = "/Installing?imageUrl=" + w.ImageUrl
                    }).ToList(),

                // Секция 3: Страны и регионы (IconCards)
                // Оставляем пока статику или можно выводить специфические записи
                IconCards = new List<ImageIconTextCardViewModel>
                {
                    new ImageIconTextCardViewModel
                    {
                        ImageUrl = dbItems.FirstOrDefault(w => w.Title.Contains("ireland"))?.ImageUrl ?? "/images/wallpapers/ireland.jpg",
                        AltText = "Ирландия",
                        IconUrl = "/images/flags/flagIreland.jpg",
                        Title = "Ирландия",
                        Description = "Пейзажи Ирландии из базы",
                        ButtonText = "Смотреть",
                        ButtonUrl = "/Installing?imageUrl=" + (dbItems.FirstOrDefault(w => w.Title.Contains("ireland"))?.ImageUrl ?? "/images/wallpapers/ireland.jpg")
                    }
                }
            };

            return View(collectModel);
        }
    }
}
