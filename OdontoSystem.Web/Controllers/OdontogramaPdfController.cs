using OdontoSystem.BLL.Services;
using OdontoSystem.Web.Filters;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;

namespace OdontoSystem.Web.Controllers
{
    /// <summary>
    /// HU-14 — Exportar odontograma a PDF. Vive separado de OdontogramaController
    /// a propósito: Rotativa hace una petición HTTP interna (VistaImprimible) que
    /// reutiliza la misma cookie de sesión que la petición externa (ExportarPDF).
    /// Si el controlador usara sesión en modo lectura-escritura (el default de MVC),
    /// las dos peticiones se bloquearían entre sí esperando el mismo candado de
    /// sesión y la exportación se quedaría cargando para siempre (deadlock).
    /// SessionStateBehavior.ReadOnly permite que ambas lean la sesión al mismo
    /// tiempo sin pelear por el candado. Ninguna de las dos acciones escribe en
    /// Session ni en TempData, así que el modo solo-lectura no les quita nada.
    /// </summary>
    [Autenticado]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class OdontogramaPdfController : Controller
    {
        private readonly OdontogramaService _service = new OdontogramaService();

        /// <summary>
        /// Dispara la generación del PDF. Rotativa hace una petición interna a
        /// VistaImprimible y convierte el HTML resultante (renderizado con
        /// wkhtmltopdf) en el archivo que se descarga aquí.
        /// </summary>
        [HttpGet]
        public ActionResult ExportarPDF(int id)
        {
            var odontograma = _service.ObtenerPorId(id);
            if (odontograma == null) return HttpNotFound();

            string apellido = odontograma.Paciente?.ApellidoPaterno ?? "paciente";
            string nombreArchivo = $"Odontograma_{apellido}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                .Replace(" ", "_");

            var pdf = new ActionAsPdf("VistaImprimible", new { id = id })
            {
                FileName = nombreArchivo,
                PageSize = Rotativa.Options.Size.A4,
                // El header Content-Type con charset=utf-8 no es suficiente: el binario
                // wkhtmltopdf necesita el encoding indicado explícitamente en su línea
                // de comandos o asume Windows-1252 y las tildes/eñes salen mal ("Ã³").
                CustomSwitches = "--encoding utf-8"
            };

            // Este proyecto autentica por Session["IdUsuario"] (AutenticadoAttribute),
            // no por FormsAuthentication. El reenvío de cookies automático de Rotativa
            // solo cubre la cookie de FormsAuth (".ASPXAUTH"), así que hay que reenviar
            // la cookie de sesión ASP.NET a mano para que la petición interna a
            // VistaImprimible no caiga en el login. "ASP.NET_SessionId" es el nombre
            // por defecto (Web.config no lo redefine con <sessionState cookieName="...">).
            var cookieSesion = Request.Cookies["ASP.NET_SessionId"];
            if (cookieSesion != null)
            {
                pdf.Cookies = new Dictionary<string, string>
                {
                    { cookieSesion.Name, cookieSesion.Value }
                };
            }

            return pdf;
        }

        /// <summary>
        /// Vista imprimible, sin layout ni JavaScript, usada exclusivamente por
        /// Rotativa/wkhtmltopdf para renderizar el PDF de HU-14. No debe enlazarse
        /// directamente desde la UI interactiva. La vista física sigue viviendo en
        /// Views/Odontograma/VistaImprimible.cshtml para no duplicar archivos.
        /// </summary>
        [HttpGet]
        public ActionResult VistaImprimible(int id)
        {
            var odontograma = _service.ObtenerPorId(id);
            if (odontograma == null) return HttpNotFound();

            // wkhtmltopdf (motor de Rotativa) prioriza el charset del header HTTP
            // Content-Type por sobre el <meta charset="utf-8"> del HTML. Sin esto,
            // interpreta los bytes UTF-8 como Windows-1252 y las tildes/eñes salen
            // como "Ã³", "â€”", etc.
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "utf-8";

            return View("~/Views/Odontograma/VistaImprimible.cshtml", odontograma);
        }
    }
}
