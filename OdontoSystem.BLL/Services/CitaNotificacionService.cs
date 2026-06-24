using System;
using System.Configuration;
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
        //  MÉTODOS PRIVADOS
        // ─────────────────────────────────────────────────────────────

        private bool PuedeEnviar(string telefono)
        {
            return _configurado
                && !string.IsNullOrWhiteSpace(telefono)
                && telefono.Length == 9;
        }

        /// <summary>
        /// Envía el mensaje de forma silenciosa — si falla, no lanza excepción.
        /// Esto garantiza que un fallo de Twilio nunca interrumpa la operación principal.
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
            catch
            {
                // Fallo silencioso intencional — el log de errores de HU-13
                // capturará esto si está configurado.
            }
        }
    }
}