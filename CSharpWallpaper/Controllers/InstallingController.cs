using Microsoft.AspNetCore.Mvc;

namespace CSharpWallpaper.Controllers
{
    [Route("[controller]")]
    public class InstallingController : Controller
    {
        [HttpGet("")]
        public IActionResult Installing()
        {
            return View();
        }
    }
}
