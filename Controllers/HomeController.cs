using ConveyorScada.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConveyorScada.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly LiveValuesCache _cache;

        public HomeController(LiveValuesCache cache)
        {
            _cache = cache;
        }

        public IActionResult Index() => View();

        /// <summary>Поточні значення для мнемосхеми (сторінка опитує цей метод щосекунди).</summary>
        public IActionResult Data()
        {
            return Json(new
            {
                sensors = _cache.GetAll(),
                state = _cache.State,
                serverTimeUtc = DateTimeOffset.UtcNow
            });
        }
    }
}
