using OdontoSystem.BLL.Services;
using OdontoSystem.Web.Filters;
using System;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    [SoloOdontologoOAdmin]
    public class EvolucionesController : Controller
    {
        private readonly EvolucionService _service = new EvolucionService();
        private readonly PlanTratamientoService _planService = new PlanTratamientoService();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(int idPlan, int idTratamiento, int idPlanDetalle,
                                       string descripcion, DateTime fechaEvolucion,
                                       int? idCita, string nuevoEstadoTratamiento)
        {
            try
            {
                int? idOdontologo = Session["IdUsuario"] as int?;
                if (idOdontologo == null)
                    return RedirectToAction("Login", "Account");

                _service.Registrar(idPlan, idTratamiento, idPlanDetalle,
                                   idOdontologo.Value, descripcion,
                                   fechaEvolucion, idCita, nuevoEstadoTratamiento);

                TempData["Exito"] = "Sesión registrada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", "Planes", new { id = idPlan });
        }
    }
}