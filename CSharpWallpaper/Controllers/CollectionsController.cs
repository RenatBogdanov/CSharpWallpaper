using Microsoft.AspNetCore.Mvc;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class CollectionsController : Controller
    {
        [HttpGet("")] // /collections
        public IActionResult Collections()
        {
            return View();
        }
    }
}
