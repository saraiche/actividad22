using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using smvcfei.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace smvcfei.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //Con el atributo authorize no se permite el acceso a no autenticados
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        //solo admisn
        [Authorize(Roles = "Administrador")]
        public IActionResult SoloParaAdministradores()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}