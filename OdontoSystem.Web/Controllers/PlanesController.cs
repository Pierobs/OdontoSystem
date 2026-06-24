using OdontoSystem.BLL.Services;
using OdontoSystem.Entities;
using OdontoSystem.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class PlanesController : Controller
    {
        private readonly PlanTratamientoService _service = new PlanTratamientoService();
        private readonly PacienteService _pacienteService = new PacienteService();
        private readonly CatalogoTratamientoService _catalogoService = new CatalogoTratamientoService();

        public ActionResult Index(int? idPaciente = null)
        {
            var planes = idPaciente.HasValue
                ? _service.ListarPorPaciente(idPaciente.Value)
                : _service.Listar();

            if (idPaciente.HasValue)
            {
                var paciente = _pacienteService.ObtenerPorId(idPaciente.Value);
                ViewBag.Paciente = paciente;
            }

            return View(planes);
        }

        public ActionResult Detalle(int id)
        {
            var plan = _service.ObtenerPorId(id);
            if (plan == null) return HttpNotFound();
            return View(plan);
        }

        [SoloOdontologoOAdmin]
        public ActionResult Crear(int? idPaciente = null)
        {
            ViewBag.Pacientes = _pacienteService.Listar().Where(p => p.Estado == "A").ToList();
            ViewBag.Tratamientos = _catalogoService.Listar(soloActivos: true).ToList();
            ViewBag.IdPacienteSeleccionado = idPaciente;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloOdontologoOAdmin]
        public ActionResult Crear(int idPaciente, int[] idTratamiento, int[] cantidad, decimal[] precioUnitario)
        {
            try
            {
                if (idTratamiento == null || idTratamiento.Length == 0)
                    throw new InvalidOperationException("Debe agregar al menos un tratamiento al plan");

                var detalles = new List<PlanDetalle>();
                for (int i = 0; i < idTratamiento.Length; i++)
                {
                    detalles.Add(new PlanDetalle
                    {
                        IdTratamiento = idTratamiento[i],
                        Cantidad = (byte)cantidad[i],
                        PrecioUnitario = precioUnitario[i]
                    });
                }

                int idPlan = _service.Crear(idPaciente, detalles);
                TempData["Exito"] = "Plan creado correctamente";
                return RedirectToAction("Detalle", new { id = idPlan });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Crear", new { idPaciente = idPaciente });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloOdontologoOAdmin]
        public ActionResult AgregarDetalle(int idPlan, int idTratamiento, int cantidad, decimal precioUnitario)
        {
            try
            {
                var detalle = new PlanDetalle
                {
                    IdTratamiento = idTratamiento,
                    Cantidad = (byte)cantidad,
                    PrecioUnitario = precioUnitario
                };
                _service.AgregarDetalle(idPlan, detalle);
                TempData["Exito"] = "Tratamiento agregado al plan";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", new { id = idPlan });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloOdontologoOAdmin]
        public ActionResult QuitarDetalle(int idPlanDetalle, int idPlan)
        {
            try
            {
                _service.QuitarDetalle(idPlanDetalle);
                TempData["Exito"] = "Tratamiento removido del plan";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", new { id = idPlan });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloOdontologoOAdmin]
        public ActionResult Cancelar(int id)
        {
            try
            {
                _service.Cancelar(id);
                TempData["Exito"] = "Plan cancelado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloOdontologoOAdmin]
        public ActionResult Cerrar(int id)
        {
            try
            {
                _service.Cerrar(id);
                TempData["Exito"] = "Plan cerrado correctamente como pagado";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", new { id = id });
        }
    }
}