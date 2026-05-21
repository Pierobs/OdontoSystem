using System;
using System.Collections.Generic;
using System.Linq;
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
        // ===== NIVEL 2.3 — LISTA BLANCA =====
        // Pon en false si prefieres aceptar cualquier dominio (solo bloqueando desechables).
        private static readonly bool UsarListaBlanca = true;

        private static readonly HashSet<string> DominiosPermitidos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com", "hotmail.com", "outlook.com",
            "yahoo.com", "yahoo.es", "icloud.com", "mail.isil.pe"
        };

        // ===== NIVEL 2.2 — DOMINIOS DESECHABLES =====
        private static readonly HashSet<string> DominiosDesechables =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tempmail.com", "mailinator.com", "10minutemail.com",
            "guerrillamail.com", "yopmail.com", "trashmail.com",
            "getnada.com", "temp-mail.org", "sharklasers.com"
        };

        // Dominios "correctos" usados para detectar errores de tipeo (Nivel 2.4)
        private static readonly string[] DominiosComunes =
        {
            "gmail.com", "hotmail.com", "outlook.com",
            "yahoo.com", "yahoo.es", "icloud.com", "mail.isil.pe"
        };

        private static readonly Regex FormatoEmail =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public static EmailValidationResult Validar(string correo, bool obligatorio = false)
        {
            var r = new EmailValidationResult { EsValido = true, Mensaje = "Correo válido" };

            // Campo vacío
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

            // NIVEL 2.2 — Bloquear desechables
            if (DominiosDesechables.Contains(dominio))
            {
                r.EsValido = false;
                r.Mensaje = "No se permiten correos de dominios temporales o desechables";
                return r;
            }

            // NIVEL 2.4 — Sugerir corrección de typo (gmial.com -> gmail.com)
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

            return r;
        }

        // Busca el dominio "bueno" más parecido (1 o 2 letras de diferencia)
        private static string SugerirDominio(string dominio)
        {
            if (DominiosComunes.Contains(dominio)) return null; // exacto, sin typo
            foreach (var bueno in DominiosComunes)
            {
                int dist = Levenshtein(dominio, bueno);
                if (dist > 0 && dist <= 2) return bueno;
            }
            return null;
        }

        // Distancia de edición entre dos textos
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