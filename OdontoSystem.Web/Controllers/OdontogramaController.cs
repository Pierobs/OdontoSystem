using OdontoSystem.BLL.Services;
using OdontoSystem.Web.Filters;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class OdontogramaController : Controller
    {
        private readonly OdontogramaService _service = new OdontogramaService();
        private readonly CitaService _citaService = new CitaService();

        [SoloOdontologoOAdmin]
        public ActionResult Atender(int idCita)
        {
            try
            {
                int idOdontograma = _service.AtenderCitaYCrearOdontograma(idCita);
                return RedirectToAction("Ver", new { id = idOdontograma });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Citas");
            }
        }

        public ActionResult Ver(int id)
        {
            var odontograma = _service.ObtenerPorId(id);
            if (odontograma == null) return HttpNotFound();
            ViewBag.EstadosPorPieza = _service.ObtenerEstadosPorPieza(id);
            ViewBag.EstadosValidos = OdontogramaService.EstadosValidos;
            ViewBag.SuperficiesValidas = OdontogramaService.SuperficiesValidas;
            ViewBag.SoloLectura = false;
            return View(odontograma);
        }

        /// <summary>
        /// Muestra el odontograma más reciente del paciente en modo solo lectura.
        /// Accesible desde el perfil del paciente sin necesidad de atender una cita.
        /// </summary>
        public ActionResult VerPaciente(int idPaciente)
        {
            var odontograma = _service.ObtenerOdontogramaActualPorPaciente(idPaciente);

            if (odontograma == null)
            {
                TempData["Error"] = "Este paciente aún no tiene odontograma registrado. " +
                                    "El odontograma se crea al atender la primera cita.";
                return RedirectToAction("Index", "Pacientes");
            }

            ViewBag.EstadosPorPieza = _service.ObtenerEstadosPorPieza(odontograma.IdOdontograma);
            ViewBag.EstadosValidos = OdontogramaService.EstadosValidos;
            ViewBag.SuperficiesValidas = OdontogramaService.SuperficiesValidas;
            ViewBag.SoloLectura = true;
            return View("Ver", odontograma);
        }

        public ActionResult PorPaciente(int idPaciente)
        {
            var odontogramas = _service.ListarPorPaciente(idPaciente);
            ViewBag.IdPaciente = idPaciente;
            return View(odontogramas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloOdontologoOAdmin]
        public JsonResult RegistrarPieza(int idOdontograma, byte numeroPieza,
                                          string estado, string superficie, string observacion)
        {
            try
            {
                _service.RegistrarPieza(idOdontograma, numeroPieza, estado, superficie, observacion);
                return Json(new { success = true, mensaje = "Pieza actualizada" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult ObtenerEstados(int idOdontograma)
        {
            try
            {
                var dientes = _service.ObtenerPorId(idOdontograma).DientesEstado
                    .Select(d => new {
                        idDienteEstado = d.IdDienteEstado,
                        numeroPieza = d.NumeroPieza,
                        superficie = d.Superficie,
                        estado = d.Estado,
                        observacion = d.Observacion
                    });
                return Json(dientes, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }
        }
    }
}