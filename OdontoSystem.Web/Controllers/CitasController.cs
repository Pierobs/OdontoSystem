using OdontoSystem.BLL.Services;
using OdontoSystem.Entities;
using OdontoSystem.Web.Filters;
using System;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class CitasController : Controller
    {
        private readonly CitaService _service = new CitaService();

        public ActionResult Index()
        {
            return View(_service.Listar());
        }

        public ActionResult Detalle(int id)
        {
            var cita = _service.ObtenerPorId(id);
            if (cita == null) return HttpNotFound();

            ViewBag.Historial = _service.ObtenerHistorial(id);
            return View(cita);
        }

        public ActionResult Crear()
        {
            ViewBag.Pacientes = _service.ListarPacientesActivos();
            ViewBag.Odontologos = _service.ListarOdontologosActivos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Cita cita)
        {
            try
            {
                _service.Agendar(cita);
                TempData["Exito"] = "Cita agendada correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Pacientes = _service.ListarPacientesActivos();
                ViewBag.Odontologos = _service.ListarOdontologosActivos();
                return View(cita);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int id, string motivo)
        {
            try
            {
                int? idUsuario = Session["IdUsuario"] as int?;
                _service.Cancelar(id, motivo, idUsuario);
                TempData["Exito"] = "Cita cancelada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reprogramar(int id, DateTime nuevaFecha, string nuevaHora, string motivo)
        {
            try
            {
                int? idUsuario = Session["IdUsuario"] as int?;
                var hora = TimeSpan.Parse(nuevaHora);
                _service.Reprogramar(id, nuevaFecha, hora, motivo, idUsuario);
                TempData["Exito"] = "Cita reprogramada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult SlotsDisponibles(DateTime fecha)
        {
            var slots = _service.ObtenerSlotsDeFecha(fecha);
            return Json(slots, JsonRequestBehavior.AllowGet);
        }
    }
}