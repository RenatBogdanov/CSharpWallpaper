using CSharpWallpaper.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class CollectionsController(IWallpaperService wallpaperService) : Controller
    {
        [HttpPost("Select")]
        public IActionResult Select([FromBody] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return BadRequest();

            wallpaperService.SaveSelectedWallpaper(imageUrl);
            return Ok(new { success = true });
        }

        [HttpGet("")]
        public IActionResult Collections(string category)
        {
            ViewBag.ActivePage = "Collections";
            ViewBag.SelectedUrl = wallpaperService.GetSelectedWallpaper();

            if (string.IsNullOrEmpty(category))
            {
                ViewBag.Title = "Все коллекции";
                var model = wallpaperService.GetCategoriesModel();
                return View(model);
            }

            ViewBag.Title = $"Коллекция: {category}";
            ViewBag.CurrentCategory = category;

            var categoryModel = wallpaperService.GetCategoryItemsModel(category);
            return View(categoryModel);
        }
    }
}
