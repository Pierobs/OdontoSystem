using OdontoSystem.BLL.Services;
using OdontoSystem.Entities;
using OdontoSystem.Web.Filters;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class CitasController : Controller
    {
        private readonly CitaService _service = new CitaService();
        private readonly CitaNotificacionService _notif = new CitaNotificacionService();

        public ActionResult Index()
        {
            ViewBag.Odontologos = _service.ListarOdontologosActivos();
            return View(_service.Listar());
        }

        public ActionResult Buscar(string criterio)
        {
            ViewBag.Odontologos = _service.ListarOdontologosActivos();
            ViewBag.Criterio = criterio;
            return View("Index", _service.Buscar(criterio));
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
            // B-05: el paciente ya no se carga completo aquí — se busca en tiempo
            // real desde Citas/Crear.cshtml vía Pacientes/BuscarPorNombre.
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

                // Notificar al paciente por WhatsApp (no bloqueante)
                var paciente = _service.ObtenerPaciente(cita.IdPaciente);
                var odontologo = _service.ObtenerOdontologo(cita.IdOdontologo);
                if (paciente != null && odontologo != null)
                {
                    _notif.NotificarCitaCreada(
                        paciente.Telefono,
                        $"{paciente.Nombres} {paciente.ApellidoPaterno}",
                        cita.FechaCita,
                        cita.HoraCita,
                        $"{odontologo.Nombres} {odontologo.ApellidoPaterno}"
                    );
                }

                TempData["Exito"] = "Cita agendada correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Odontologos = _service.ListarOdontologosActivos();

                // B-05: si ya se había seleccionado un paciente antes de que fallara
                // la validación, recuperamos su nombre para no perder la selección
                // visual en el input de búsqueda (el IdPaciente ya viaja en cita).
                if (cita.IdPaciente > 0)
                {
                    var pacienteSeleccionado = _service.ObtenerPaciente(cita.IdPaciente);
                    if (pacienteSeleccionado != null)
                        ViewBag.PacienteSeleccionadoTexto =
                            $"{pacienteSeleccionado.ApellidoPaterno} {pacienteSeleccionado.ApellidoMaterno}, " +
                            $"{pacienteSeleccionado.Nombres} — {pacienteSeleccionado.NumeroDocumento}";
                }

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

                // Obtener datos antes de cancelar para la notificación
                var cita = _service.ObtenerPorId(id);
                _service.Cancelar(id, motivo, idUsuario);

                // Notificar al paciente por WhatsApp (no bloqueante)
                if (cita?.Paciente != null)
                {
                    _notif.NotificarCitaCancelada(
                        cita.Paciente.Telefono,
                        $"{cita.Paciente.Nombres} {cita.Paciente.ApellidoPaterno}",
                        cita.FechaCita,
                        cita.HoraCita,
                        motivo
                    );

                    // HU-15, criterio E3: advertir si no se pudo notificar por falta de teléfono
                    if (!_notif.TieneTelefonoValido(cita.Paciente.Telefono))
                        TempData["Advertencia"] = "El paciente no tiene número de teléfono registrado. No se pudo enviar la notificación.";
                }

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
        public ActionResult Reprogramar(int id, DateTime nuevaFecha, string nuevaHora,
                                 string motivo, int? nuevoIdOdontologo = null)
        {
            try
            {
                int? idUsuario = Session["IdUsuario"] as int?;
                var hora = TimeSpan.Parse(nuevaHora);

                var cita = _service.ObtenerPorId(id);
                DateTime fechaAnterior = cita?.FechaCita ?? DateTime.Now;
                TimeSpan horaAnterior = cita?.HoraCita ?? TimeSpan.Zero;

                _service.Reprogramar(id, nuevaFecha, hora, motivo, idUsuario, nuevoIdOdontologo);

                if (cita?.Paciente != null)
                {
                    _notif.NotificarCitaReprogramada(
                        cita.Paciente.Telefono,
                        $"{cita.Paciente.Nombres} {cita.Paciente.ApellidoPaterno}",
                        fechaAnterior, horaAnterior,
                        nuevaFecha, hora, motivo
                    );

                    // HU-15, criterio E3: advertir si no se pudo notificar por falta de teléfono
                    if (!_notif.TieneTelefonoValido(cita.Paciente.Telefono))
                        TempData["Advertencia"] = "El paciente no tiene número de teléfono registrado. No se pudo enviar la notificación.";
                }

                TempData["Exito"] = "Cita reprogramada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult SlotsDisponibles(DateTime fecha, int? idOdontologo = null)
        {
            // Si se pasa idOdontologo, devuelve slots del calendario de ese odontólogo
            if (idOdontologo.HasValue)
            {
                var dispService = new DisponibilidadService();
                var slotsOdontologo = dispService.SlotsDisponiblesOdontologo(idOdontologo.Value, fecha);

                // Mapear a formato compatible con el JS existente
                var citasDelDia = _service.ListarCitasDelDia(fecha, idOdontologo.Value);
                var resultado = slotsOdontologo.Select(s => new
                {
                    hora = s.ToString(@"hh\:mm"),
                    ocupados = citasDelDia.Count(c => c.HoraCita == s),
                    disponible = true // Ya vienen filtrados por DisponibilidadService
                });
                return Json(resultado, JsonRequestBehavior.AllowGet);
            }

            // Sin odontólogo: comportamiento anterior (slots globales del consultorio)
            var slots = _service.ObtenerSlotsDeFecha(fecha);
            return Json(slots, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FechasDisponibles(int idOdontologo)
        {
            var dispService = new DisponibilidadService();
            // Buscar disponibilidad en los próximos 60 días
            var desde = DateTime.Today;
            var hasta = DateTime.Today.AddDays(60);
            var disponibilidades = dispService.Listar(idOdontologo, desde, hasta);
            var fechas = disponibilidades
                .Select(d => d.Fecha.ToString("yyyy-MM-dd"))
                .Distinct()
                .OrderBy(f => f)
                .ToList();
            return Json(fechas, JsonRequestBehavior.AllowGet);
        }
    }
}