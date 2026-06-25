using OdontoSystem.BLL.Services;
using OdontoSystem.Web.Filters;
using System;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class PagosController : Controller
    {
        private readonly PagoService _service = new PagoService();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(int idPlan, decimal monto, string metodoPago)
        {
            try
            {
                int? idUsuario = Session["IdUsuario"] as int?;
                if (idUsuario == null)
                    return RedirectToAction("Login", "Account");

                _service.Registrar(idPlan, monto, metodoPago, idUsuario.Value);
                TempData["Exito"] = $"Pago de S/. {monto:N2} registrado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", "Planes", new { id = idPlan });
        }
    }
}