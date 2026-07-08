using System;
using System.Configuration;
using System.IO;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OdontoSystem.BLL.Services
{
    /// <summary>
    /// Servicio de notificaciones WhatsApp para eventos de citas.
    /// Envía mensajes informativos (no OTP) al paciente cuando
    /// se crea, cancela o reprograma una cita.
    /// El envío es no bloqueante: si Twilio falla, registra el error
    /// pero no interrumpe la operación principal.
    /// </summary>
    public class CitaNotificacionService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _whatsAppFrom;
        private readonly bool _configurado;

        public CitaNotificacionService()
        {
            _accountSid  = ConfigurationManager.AppSettings["Twilio:AccountSid"];
            _authToken   = ConfigurationManager.AppSettings["Twilio:AuthToken"];
            _whatsAppFrom = ConfigurationManager.AppSettings["Twilio:WhatsAppFrom"];

            _configurado = !string.IsNullOrWhiteSpace(_accountSid)
                        && !string.IsNullOrWhiteSpace(_authToken)
                        && !string.IsNullOrWhiteSpace(_whatsAppFrom);

            if (_configurado)
                TwilioClient.Init(_accountSid, _authToken);
        }

        /// <summary>
        /// Notifica al paciente que su cita fue agendada.
        /// </summary>
        public void NotificarCitaCreada(string telefonoPaciente, string nombrePaciente,
                                         DateTime fecha, TimeSpan hora, string nombreOdontologo)
        {
            if (!PuedeEnviar(telefonoPaciente)) return;

            string mensaje =
                $"🦷 *Consultorio Odontológico Romero*\n\n" +
                $"Hola {nombrePaciente}, tu cita ha sido *agendada* correctamente:\n\n" +
                $"📅 Fecha: {fecha:dd/MM/yyyy}\n" +
                $"⏰ Hora: {hora:hh\\:mm}\n" +
                $"👨‍⚕️ Odontólogo: Dr. {nombreOdontologo}\n\n" +
                $"Por favor llega 10 minutos antes. Cualquier consulta comunícate con nosotros.";

            EnviarSilencioso(telefonoPaciente, mensaje);
        }

        /// <summary>
        /// Notifica al paciente que su cita fue cancelada.
        /// </summary>
        public void NotificarCitaCancelada(string telefonoPaciente, string nombrePaciente,
                                            DateTime fecha, TimeSpan hora, string motivo)
        {
            if (!PuedeEnviar(telefonoPaciente)) return;

            string mensaje =
                $"🦷 *Consultorio Odontológico Romero*\n\n" +
                $"Hola {nombrePaciente}, lamentamos informarte que tu cita ha sido *cancelada*:\n\n" +
                $"📅 Fecha cancelada: {fecha:dd/MM/yyyy}\n" +
                $"⏰ Hora: {hora:hh\\:mm}\n" +
                $"📝 Motivo: {motivo}\n\n" +
                $"Comunícate con nosotros para reagendar tu cita.";

            EnviarSilencioso(telefonoPaciente, mensaje);
        }

        /// <summary>
        /// Notifica al paciente que su cita fue reprogramada.
        /// </summary>
        public void NotificarCitaReprogramada(string telefonoPaciente, string nombrePaciente,
                                               DateTime fechaAnterior, TimeSpan horaAnterior,
                                               DateTime fechaNueva, TimeSpan horaNueva,
                                               string motivo)
        {
            if (!PuedeEnviar(telefonoPaciente)) return;

            string mensaje =
                $"🦷 *Consultorio Odontológico Romero*\n\n" +
                $"Hola {nombrePaciente}, tu cita ha sido *reprogramada*:\n\n" +
                $"❌ Fecha anterior: {fechaAnterior:dd/MM/yyyy} {horaAnterior:hh\\:mm}\n" +
                $"✅ Nueva fecha: {fechaNueva:dd/MM/yyyy} {horaNueva:hh\\:mm}\n" +
                $"📝 Motivo: {motivo}\n\n" +
                $"Por favor llega 10 minutos antes. Cualquier consulta comunícate con nosotros.";

            EnviarSilencioso(telefonoPaciente, mensaje);
        }

        // ─────────────────────────────────────────────────────────────
        //  MÉTODOS PÚBLICOS DE APOYO (HU-15, criterio E3)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Indica si el paciente tiene un número de teléfono con formato válido
        /// para recibir notificaciones, independientemente de si Twilio está
        /// configurado. Los controllers usan esto para mostrar la advertencia
        /// visual del criterio E3 de HU-15 ("El paciente no tiene número de
        /// teléfono registrado. No se pudo enviar la notificación.").
        /// </summary>
        public bool TieneTelefonoValido(string telefono)
        {
            return !string.IsNullOrWhiteSpace(telefono) && telefono.Trim().Length == 9;
        }

        // ─────────────────────────────────────────────────────────────
        //  MÉTODOS PRIVADOS
        // ─────────────────────────────────────────────────────────────

        private bool PuedeEnviar(string telefono)
        {
            return _configurado && TieneTelefonoValido(telefono);
        }

        /// <summary>
        /// Envía el mensaje de forma silenciosa — si falla, no lanza excepción.
        /// Esto garantiza que un fallo de Twilio nunca interrumpa la operación principal.
        /// El fallo, sin embargo, sí queda registrado en el mismo log de HU-13
        /// (App_Data/Logs/errores_yyyy-MM-dd.log) para que sea auditable.
        /// </summary>
        private void EnviarSilencioso(string telefono, string mensaje)
        {
            try
            {
                MessageResource.Create(
                    from: new PhoneNumber(_whatsAppFrom),
                    to:   new PhoneNumber($"whatsapp:+51{telefono}"),
                    body: mensaje
                );
            }
            catch (Exception ex)
            {
                // Fallo silencioso hacia el usuario (HU-15, criterio: "no muestra
                // error al usuario"), pero registrado en log (HU-15, criterio:
                // "registra el fallo en el log").
                RegistrarFalloEnLog(telefono, ex);
            }
        }

        /// <summary>
        /// Registra el fallo de envío de WhatsApp en el mismo archivo de log
        /// que usa HU-13 (ManejadorErrorGlobalAttribute), sin depender de
        /// System.Web ya que esta clase vive en la capa BLL.
        /// </summary>
        private void RegistrarFalloEnLog(string telefono, Exception ex)
        {
            try
            {
                string carpetaLogs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data", "Logs");
                carpetaLogs = Path.GetFullPath(carpetaLogs);
                if (!Directory.Exists(carpetaLogs))
                    Directory.CreateDirectory(carpetaLogs);

                string archivo = Path.Combine(carpetaLogs, $"errores_{DateTime.Now:yyyy-MM-dd}.log");

                string contenido =
                    $"========================================{Environment.NewLine}" +
                    $"Origen:    HU-15 — Notificación WhatsApp (fallo no bloqueante){Environment.NewLine}" +
                    $"Fecha:     {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
                    $"Teléfono:  {telefono}{Environment.NewLine}" +
                    $"Mensaje:   {ex.Message}{Environment.NewLine}" +
                    $"Tipo:      {ex.GetType().FullName}{Environment.NewLine}" +
                    $"========================================{Environment.NewLine}{Environment.NewLine}";

                File.AppendAllText(archivo, contenido);
            }
            catch
            {
                // Si falla el log, no romper la app — igual criterio que HU-13.
            }
        }
    }
}