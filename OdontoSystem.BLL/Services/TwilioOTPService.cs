using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Configuration;
using System.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OdontoSystem.BLL.Services
{
    public class TwilioOTPService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _whatsAppFrom;

        public TwilioOTPService()
        {
            _accountSid = ConfigurationManager.AppSettings["Twilio:AccountSid"];
            _authToken = ConfigurationManager.AppSettings["Twilio:AuthToken"];
            _whatsAppFrom = ConfigurationManager.AppSettings["Twilio:WhatsAppFrom"];
            TwilioClient.Init(_accountSid, _authToken);
        }

        public class EnvioOTPResult
        {
            public bool Exito { get; set; }
            public string Mensaje { get; set; }
        }

        public class VerificacionOTPResult
        {
            public bool Exito { get; set; }
            public string Mensaje { get; set; }
        }

        /// <summary>
        /// Genera y envía un código OTP de 6 dígitos al WhatsApp del usuario.
        /// El código expira en 5 minutos.
        /// </summary>
        public EnvioOTPResult EnviarCodigo(string telefono)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(telefono))
                    return new EnvioOTPResult { Exito = false, Mensaje = "Teléfono vacío" };

                using (var ctx = new OdontoContext())
                {
                    // Invalidar OTPs anteriores del mismo teléfono
                    var anteriores = ctx.TelefonoOTPs
                        .Where(o => o.Telefono == telefono && !o.Verificado)
                        .ToList();
                    foreach (var a in anteriores) a.Verificado = true;

                    // Generar nuevo código
                    var random = new Random();
                    string codigo = random.Next(100000, 999999).ToString();

                    ctx.TelefonoOTPs.Add(new TelefonoOTP
                    {
                        Telefono = telefono,
                        Codigo = codigo,
                        FechaCreacion = DateTime.Now,
                        FechaExpiracion = DateTime.Now.AddMinutes(5),
                        Verificado = false,
                        Intentos = 0
                    });
                    ctx.SaveChanges();

                    // Enviar por WhatsApp Twilio
                    string mensajeTexto = $"Tu código de verificación OdontoSystem es: {codigo}\n\nEste código expira en 5 minutos. No lo compartas con nadie.";

                    var mensaje = MessageResource.Create(
                        from: new PhoneNumber(_whatsAppFrom),
                        to: new PhoneNumber($"whatsapp:+51{telefono}"),
                        body: mensajeTexto
                    );

                    return new EnvioOTPResult
                    {
                        Exito = true,
                        Mensaje = "Código enviado por WhatsApp. Revisa tu celular."
                    };
                }
            }
            catch (Exception ex)
            {
                return new EnvioOTPResult
                {
                    Exito = false,
                    Mensaje = "Error al enviar: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Verifica el código ingresado por el usuario.
        /// </summary>
        public VerificacionOTPResult VerificarCodigo(string telefono, string codigo)
        {
            using (var ctx = new OdontoContext())
            {
                var otp = ctx.TelefonoOTPs
                    .Where(o => o.Telefono == telefono && !o.Verificado)
                    .OrderByDescending(o => o.FechaCreacion)
                    .FirstOrDefault();

                if (otp == null)
                    return new VerificacionOTPResult { Exito = false, Mensaje = "No hay código activo. Solicita uno nuevo." };

                if (otp.FechaExpiracion < DateTime.Now)
                    return new VerificacionOTPResult { Exito = false, Mensaje = "El código ha expirado. Solicita uno nuevo." };

                if (otp.Intentos >= 3)
                    return new VerificacionOTPResult { Exito = false, Mensaje = "Demasiados intentos. Solicita un código nuevo." };

                otp.Intentos++;

                if (otp.Codigo != codigo)
                {
                    ctx.SaveChanges();
                    return new VerificacionOTPResult { Exito = false, Mensaje = "Código incorrecto" };
                }

                otp.Verificado = true;
                ctx.SaveChanges();
                return new VerificacionOTPResult { Exito = true, Mensaje = "Teléfono verificado correctamente" };
            }
        }
    }
}
