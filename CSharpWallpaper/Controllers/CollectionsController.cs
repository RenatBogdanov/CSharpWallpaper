using CSharpWallpaper.Models;
using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class CollectionsController : Controller
    {
        private readonly AppDbContext _context;

        public CollectionsController(AppDbContext context)
        {
            _context = context;
        }

        // МЕТОД ВЫБОРА (JS AJAX)
        // Позволяет выбрать обои без перезагрузки всей страницы
        [HttpPost("Select")]
        public IActionResult Select([FromBody] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return BadRequest();

            // Сохраняем выбор в Cookies на стороне сервера (через фоновый запрос)
            CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
            Response.Cookies.Append("LastSelectedWallpaper", imageUrl, option);

            return Ok(new { success = true });
        }

        [HttpGet("")]
        public IActionResult Collections(string category)
        {
            ViewBag.ActivePage = "Collections";

            // Читаем из куки текущий выбор, чтобы пометить карточку во View при загрузке
            ViewBag.SelectedUrl = Request.Cookies["LastSelectedWallpaper"];

            var dbItems = _context.Wallpapers.ToList();

            // Если категория не выбрана — показываем список папок
            if (string.IsNullOrEmpty(category))
            {
                ViewBag.Title = "Все коллекции";
                var categoriesModel = new WallpaperCollectionViewModel
                {
                    ImageTextCards = dbItems
                        .GroupBy(w => w.Category)
                        .Select(g => g.First())
                        .Select(w => new ImageTextCardViewModel
                        {
                            ImageUrl = w.ImageUrl,
                            Title = w.Category ?? "Общие",
                            Description = $"Смотреть обои из папки {w.Category}",
                            ClickUrl = $"/Collections?category={w.Category}"
                        }).ToList(),
                    SimpleCards = new List<SimpleImageCardViewModel>(),
                    IconCards = new List<ImageIconTextCardViewModel>()
                };
                return View(categoriesModel);
            }

            // Если категория выбрана — показываем обои этой категории
            ViewBag.Title = $"Коллекция: {category}";
            ViewBag.CurrentCategory = category;

            var specificCollection = new WallpaperCollectionViewModel
            {
                SimpleCards = dbItems
                    .Where(w => w.Category == category)
                    .Select(w => new SimpleImageCardViewModel
                    {
                        ImageUrl = w.ImageUrl,
                        AltText = w.Title,
                        // Теперь ClickUrl не нужен для перехода, выбор идет через JS onclick
                        ClickUrl = "#"
                    }).ToList(),
                ImageTextCards = new List<ImageTextCardViewModel>(),
                IconCards = new List<ImageIconTextCardViewModel>()
            };

            return View(specificCollection);
        }
    }
}
