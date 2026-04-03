using Microsoft.AspNetCore.Mvc;
using CSharpWallpaper.Models;
using CSharpWallpaper.Data; // ДОБАВИТЬ ЭТО
using System.Linq;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class MainController : Controller
    {
        private readonly AppDbContext _context;

        // Внедряем контекст в конструктор
        public MainController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [Route("/")]
        public IActionResult Main()
        {
            // Берем всё из базы
            var dbItems = _context.Wallpapers.ToList();

            var viewModel = new WallpaperCollectionViewModel
            {
                // Заполняем список Дани данными из БД
                SimpleCards = dbItems.Select(w => new SimpleImageCardViewModel
                {
                    ImageUrl = w.ImageUrl,
                    AltText = w.Title
                }).ToList(),

                // Оставляем пустые списки или заполняем их также
                ImageTextCards = new List<ImageTextCardViewModel>(),
                IconCards = new List<ImageIconTextCardViewModel>()
            };

            return View(viewModel);
        }
    }
}