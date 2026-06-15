using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace OdontoSystem.BLL.Services
{
    public class VeriphoneService
    {
        private static readonly HttpClient _http = new HttpClient();
        private readonly string _apiKey;

        public VeriphoneService()
        {
            _apiKey = ConfigurationManager.AppSettings["Veriphone:ApiKey"];
        }

        public class ValidacionTelefonoResult
        {
            public bool Valido { get; set; }
            public string Mensaje { get; set; }
            public string Operadora { get; set; }
            public string TipoLinea { get; set; }
            public string Pais { get; set; }
            public string FormatoInternacional { get; set; }
        }

        public async Task<ValidacionTelefonoResult> ValidarAsync(string telefono)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    return new ValidacionTelefonoResult
                    {
                        Valido = false,
                        Mensaje = "API Key de Veriphone no configurada"
                    };
                }

                // Veriphone espera el número en formato internacional: +51 + 9 dígitos
                string numeroCompleto = "+51" + telefono;
                string url = $"https://api.veriphone.io/v2/verify?phone={Uri.EscapeDataString(numeroCompleto)}&default_country=PE&key={_apiKey}";

                var response = await _http.GetStringAsync(url);
                dynamic data = JsonConvert.DeserializeObject(response);

                // Veriphone retorna phone_valid: true/false
                bool valido = data.phone_valid != null && (bool)data.phone_valid;

                if (!valido)
                {
                    return new ValidacionTelefonoResult
                    {
                        Valido = false,
                        Mensaje = "El número no existe o no es válido en Perú"
                    };
                }

                string tipoLinea = (string)data.phone_type ?? "unknown";

                // Solo aceptamos móviles
                if (tipoLinea != "mobile")
                {
                    return new ValidacionTelefonoResult
                    {
                        Valido = false,
                        Mensaje = $"Debe ingresar un número móvil (detectado: {tipoLinea})"
                    };
                }

                return new ValidacionTelefonoResult
                {
                    Valido = true,
                    Mensaje = "Número válido y verificado",
                    Operadora = (string)data.carrier,
                    TipoLinea = tipoLinea,
                    Pais = (string)data.country,
                    FormatoInternacional = (string)data.international_number
                };
            }
            catch (Exception ex)
            {
                return new ValidacionTelefonoResult
                {
                    Valido = false,
                    Mensaje = "No se pudo validar el número: " + ex.Message
                };
            }
        }
    }
}