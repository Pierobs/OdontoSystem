using System;
using System.Web.Mvc;
using OdontoSystem.BLL.Services;
using OdontoSystem.Entities;
using OdontoSystem.Web.Filters;
using System.Threading.Tasks;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class PacientesController : Controller
    {
        private readonly PacienteService _service = new PacienteService();

        public ActionResult Index()
        {
            return View(_service.Listar());
        }

        public ActionResult Crear()
        {
            // Cargar distritos para el dropdown
            ViewBag.Distritos = _service.ListarDistritos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Paciente paciente)
        {
            try
            {
                _service.Registrar(paciente);
                TempData["Exito"] = "Paciente registrado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Distritos = _service.ListarDistritos();
                return View(paciente);
            }
        }

        public ActionResult Editar(int id)
        {
            var paciente = _service.ObtenerPorId(id);
            if (paciente == null)
                return HttpNotFound();

            ViewBag.Distritos = _service.ListarDistritos();
            return View(paciente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Paciente paciente)
        {
            try
            {
                _service.Actualizar(paciente);
                TempData["Exito"] = "Paciente actualizado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Distritos = _service.ListarDistritos();
                return View(paciente);
            }
        }

        public ActionResult CambiarEstado(int id)
        {
            try
            {
                _service.CambiarEstado(id);
                TempData["Exito"] = "Estado del paciente cambiado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        public ActionResult Buscar(string criterio)
        {
            try
            {
                return View("Index", _service.Buscar(criterio));
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ValidarCorreo(string correo)
        {
            var resultado = EmailValidator.Validar(correo);
            return Json(new
            {
                valido = resultado.EsValido,
                mensaje = resultado.Mensaje,
                sugerencia = resultado.TieneSugerencia,
                correoSugerido = resultado.CorreoSugerido
            });
        }
        [HttpGet]
        public async Task<ActionResult> ConsultarDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8)
                return Json(new { success = false, mensaje = "DNI debe tener 8 dígitos" },
                            JsonRequestBehavior.AllowGet);

            var servicio = new ReniecService();
            var resultado = await servicio.ConsultarDniAsync(dni);

            if (resultado == null)
                return Json(new { success = false, mensaje = "DNI no encontrado en RENIEC" },
                            JsonRequestBehavior.AllowGet);

            return Json(new
            {
                success = true,
                nombre = resultado.first_name,
                apPaterno = resultado.first_last_name,
                apMaterno = resultado.second_last_name
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult ValidarDocumento(string numero)
        {
            bool existe = _service.ExisteDocumento(numero);
            return Json(new { existe }, JsonRequestBehavior.AllowGet);
        }
    }
}