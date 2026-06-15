using OdontoSystem.BLL.Services;
using OdontoSystem.Web.Filters;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class DisponibilidadController : Controller
    {
        private readonly DisponibilidadService _service = new DisponibilidadService();

        public ActionResult Index(int? idOdontologo = null)
        {
            string rol = Session["Rol"]?.ToString() ?? "";
            int idUsuario = (int)Session["IdUsuario"];

            // El odontólogo solo ve su propio calendario
            if (rol == "Odontologo")
            {
                idOdontologo = idUsuario;
            }
            else if (rol == "Administrador")
            {
                // Admin puede ver el de cualquiera; si no eligió, mostrar lista
                if (!idOdontologo.HasValue)
                {
                    ViewBag.Odontologos = _service.ListarOdontologos();
                    return View("SeleccionarOdontologo");
                }
            }
            else
            {
                TempData["Error"] = "No tiene permisos para acceder a esta sección";
                return RedirectToAction("Index", "Home");
            }

            var odontologo = _service.ListarOdontologos()
                .FirstOrDefault(o => o.IdUsuario == idOdontologo.Value);

            if (odontologo == null)
            {
                TempData["Error"] = "Odontólogo no encontrado";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Odontologo = odontologo;
            return View();
        }

        [HttpGet]
        public JsonResult Eventos(int idOdontologo, DateTime start, DateTime end)
        {
            try
            {
                var bloques = _service.Listar(idOdontologo, start, end);
                var eventos = bloques.Select(b => new
                {
                    id = b.IdDisponibilidad,
                    title = "Disponible",
                    start = b.Fecha.ToString("yyyy-MM-dd") + "T" + b.HoraInicio.ToString(@"hh\:mm"),
                    end = b.Fecha.ToString("yyyy-MM-dd") + "T" + b.HoraFin.ToString(@"hh\:mm"),
                    color = "#4CAF50",
                    borderColor = "#388E3C"
                });
                return Json(eventos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Crear(int idOdontologo, DateTime fecha, string horaInicio, string horaFin)
        {
            try
            {
                int id = _service.Crear(idOdontologo, fecha,
                    TimeSpan.Parse(horaInicio), TimeSpan.Parse(horaFin));
                return Json(new { success = true, id = id, mensaje = "Bloque agregado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Eliminar(int idDisponibilidad)
        {
            try
            {
                int idUsuario = (int)Session["IdUsuario"];
                bool esAdmin = Session["Rol"]?.ToString() == "Administrador";

                _service.Eliminar(idDisponibilidad, idUsuario, esAdmin);
                return Json(new { success = true, mensaje = "Bloque eliminado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ReplicarSemana(int idOdontologo, DateTime fechaInicioSemana, int semanas)
        {
            try
            {
                int replicados = _service.ReplicarSemana(idOdontologo, fechaInicioSemana, semanas);
                return Json(new
                {
                    success = true,
                    mensaje = $"Se replicaron {replicados} bloque(s) en las próximas {semanas} semana(s)"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }
    }
}