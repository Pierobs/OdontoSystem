using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace OdontoSystem.BLL.Services
{
    public class EmailValidationResult
    {
        public bool EsValido { get; set; }
        public string Mensaje { get; set; }
        public bool TieneSugerencia { get; set; }
        public string CorreoSugerido { get; set; }
    }

    public static class EmailValidator
    {
        private static readonly bool UsarListaBlanca = true;

        private static readonly HashSet<string> DominiosPermitidos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com", "hotmail.com", "outlook.com",
            "yahoo.com", "yahoo.es", "icloud.com", "mail.isil.pe"
        };

        private static readonly HashSet<string> DominiosDesechables =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tempmail.com", "mailinator.com", "10minutemail.com",
            "guerrillamail.com", "yopmail.com", "trashmail.com",
            "getnada.com", "temp-mail.org", "sharklasers.com"
        };

        private static readonly string[] DominiosComunes =
        {
            "gmail.com", "hotmail.com", "outlook.com",
            "yahoo.com", "yahoo.es", "icloud.com", "mail.isil.pe"
        };

        private static readonly Regex FormatoEmail =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        // Caché para no consumir cuota innecesaria
        private static readonly Dictionary<string, bool> CacheCorreos =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private static readonly HttpClient _http = new HttpClient();

        public static EmailValidationResult Validar(string correo, bool obligatorio = false)
        {
            var r = new EmailValidationResult { EsValido = true, Mensaje = "Correo válido" };

            if (string.IsNullOrWhiteSpace(correo))
            {
                if (obligatorio) { r.EsValido = false; r.Mensaje = "El correo es obligatorio"; }
                return r;
            }

            correo = correo.Trim().ToLowerInvariant();

            // NIVEL 1 — Formato
            if (!FormatoEmail.IsMatch(correo))
            {
                r.EsValido = false;
                r.Mensaje = "Formato de correo inválido (debe ser ejemplo@dominio.com)";
                return r;
            }

            string usuario = correo.Split('@')[0];
            string dominio = correo.Split('@')[1];

            // NIVEL 2.2 — Desechables
            if (DominiosDesechables.Contains(dominio))
            {
                r.EsValido = false;
                r.Mensaje = "No se permiten correos de dominios temporales o desechables";
                return r;
            }

            // NIVEL 2.4 — Sugerir typo
            string sugerido = SugerirDominio(dominio);
            if (sugerido != null)
            {
                r.TieneSugerencia = true;
                r.CorreoSugerido = usuario + "@" + sugerido;
                r.EsValido = false;
                r.Mensaje = $"¿Quisiste decir {r.CorreoSugerido}?";
                return r;
            }

            // NIVEL 2.3 — Lista blanca
            if (UsarListaBlanca && !DominiosPermitidos.Contains(dominio))
            {
                r.EsValido = false;
                r.Mensaje = $"El dominio '{dominio}' no está permitido. " +
                            "Use un correo de: " + string.Join(", ", DominiosPermitidos);
                return r;
            }

            // NIVEL 3 — Verificación REAL con AbstractAPI
            var verificacion = VerificarConAbstractApi(correo);
            if (!verificacion.EsValido)
                return verificacion;

            return r;
        }

        // ===== NIVEL 3 — ABSTRACTAPI =====
        private static EmailValidationResult VerificarConAbstractApi(string correo)
        {
            var r = new EmailValidationResult { EsValido = true, Mensaje = "Correo válido" };

            // Revisar caché para no gastar cuota
            if (CacheCorreos.TryGetValue(correo, out bool cacheado))
            {
                if (!cacheado)
                {
                    r.EsValido = false;
                    r.Mensaje = "Este correo no existe o no se puede entregar";
                }
                return r;
            }

            try
            {
                string apiKey = ConfigurationManager.AppSettings["AbstractApi:EmailKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                    return r; // Si no hay API key, no bloqueamos

                string url = $"https://emailvalidation.abstractapi.com/v1/?api_key={apiKey}&email={correo}";
                var response = _http.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                    return r; // No bloqueamos si la API falla

                string json = response.Content.ReadAsStringAsync().Result;
                var data = JObject.Parse(json);

                string deliverability = (string)data["deliverability"];     // DELIVERABLE / UNDELIVERABLE / UNKNOWN / RISKY
                bool isValidFormat = (bool)(data["is_valid_format"]?["value"] ?? false);
                bool isMxFound = (bool)(data["is_mx_found"]?["value"] ?? false);
                bool isSmtpValid = (bool)(data["is_smtp_valid"]?["value"] ?? false);
                bool isDisposable = (bool)(data["is_disposable_email"]?["value"] ?? false);

                if (isDisposable)
                {
                    CacheCorreos[correo] = false;
                    r.EsValido = false;
                    r.Mensaje = "No se permiten correos desechables o temporales";
                    return r;
                }

                if (!isMxFound)
                {
                    CacheCorreos[correo] = false;
                    r.EsValido = false;
                    r.Mensaje = "El dominio del correo no existe";
                    return r;
                }

                if (deliverability == "UNDELIVERABLE")
                {
                    CacheCorreos[correo] = false;
                    r.EsValido = false;
                    r.Mensaje = "Este correo no existe o no se puede entregar";
                    return r;
                }

                CacheCorreos[correo] = true;
                return r;
            }
            catch
            {
                // Si falla la API, no bloqueamos el registro
                return r;
            }
        }

        private static string SugerirDominio(string dominio)
        {
            if (DominiosComunes.Contains(dominio)) return null;
            foreach (var bueno in DominiosComunes)
            {
                int dist = Levenshtein(dominio, bueno);
                if (dist > 0 && dist <= 2) return bueno;
            }
            return null;
        }

        private static int Levenshtein(string a, string b)
        {
            int[,] dp = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) dp[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                {
                    int costo = a[i - 1] == b[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                                        dp[i - 1, j - 1] + costo);
                }
            return dp[a.Length, b.Length];
        }
    }
}