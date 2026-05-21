using System.Web.Mvc;
using OdontoSystem.BLL.Services;
using OdontoSystem.Web.Filters;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class HomeController : Controller
    {
        private readonly DashboardService _dashboard = new DashboardService();

        public ActionResult Index()
        {
            var stats = _dashboard.ObtenerEstadisticas();

            ViewBag.TotalPacientes = stats.TotalPacientes;
            ViewBag.CitasPendientes = stats.CitasPendientes;
            ViewBag.CitasHoy = stats.CitasHoy;
            ViewBag.Tratamientos = stats.Tratamientos;
            ViewBag.TotalUsuarios = stats.TotalUsuarios;
            ViewBag.Error = stats.Error;

            return View();
        }

        public ActionResult About() { return View(); }
        public ActionResult Contact() { return View(); }
    }
}