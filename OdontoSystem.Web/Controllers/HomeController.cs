using System;
using System.Linq;
using System.Web.Mvc;
using OdontoSystem.BLL.Services;
using OdontoSystem.Web.Filters;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class HomeController : Controller
    {
        private readonly DashboardService _dashboard = new DashboardService();
        private readonly DisponibilidadService _dispService = new DisponibilidadService();

        public ActionResult Index()
        {
            var stats = _dashboard.ObtenerEstadisticas();
            ViewBag.TotalPacientes = stats.TotalPacientes;
            ViewBag.CitasPendientes = stats.CitasPendientes;
            ViewBag.CitasHoy = stats.CitasHoy;
            ViewBag.Tratamientos = stats.Tratamientos;
            ViewBag.TotalUsuarios = stats.TotalUsuarios;
            ViewBag.Error = stats.Error;

            // Alerta de disponibilidad para odontólogos
            string rol = Session["Rol"]?.ToString();
            if (rol == "Odontologo")
            {
                int idUsuario = (int)Session["IdUsuario"];
                // Verificar si tiene disponibilidad registrada para esta semana
                var hoy = DateTime.Today;
                var inicioSem = hoy.AddDays(-(int)hoy.DayOfWeek + 1); // Lunes
                var finSem = inicioSem.AddDays(6);                   // Domingo

                var dispEstaSemana = _dispService.Listar(idUsuario, inicioSem, finSem);
                if (!dispEstaSemana.Any())
                    ViewBag.AlertaDisponibilidad = true;
            }

            return View();
        }

        public ActionResult About() { return View(); }
        public ActionResult Contact() { return View(); }
    }
}