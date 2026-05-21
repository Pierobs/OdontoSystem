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

        public ActionResult Cancelar(int id)
        {
            try
            {
                _service.Cancelar(id);
                TempData["Exito"] = "Cita cancelada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}