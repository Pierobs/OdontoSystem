using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OdontoSystem.Web.Filters
{
    public class ManejadorErrorGlobalAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            // 1. Generar un código único para identificar este error
            string codigoError = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            // 2. Registrar el error en archivo de log
            RegistrarError(filterContext.Exception, codigoError, filterContext.HttpContext);

            // 3. Marcar la excepción como manejada (para que no la muestre ASP.NET)
            filterContext.ExceptionHandled = true;

            // 4. Redirigir a la página de error 500 con el código
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    { "controller", "Error" },
                    { "action", "Error500" },
                    { "codigo", codigoError }
                });
        }

        private void RegistrarError(Exception ex, string codigo, HttpContextBase contexto)
        {
            try
            {
                // Carpeta donde guardar los logs
                string carpetaLogs = HttpContext.Current.Server.MapPath("~/App_Data/Logs");
                if (!Directory.Exists(carpetaLogs))
                    Directory.CreateDirectory(carpetaLogs);

                // Archivo de log por día
                string archivo = Path.Combine(carpetaLogs, $"errores_{DateTime.Now:yyyy-MM-dd}.log");

                // Datos de contexto
                string usuario = contexto.Session?["UsuarioCorreo"]?.ToString() ?? "Anónimo";
                string url = contexto.Request?.Url?.ToString() ?? "Desconocida";

                // Texto del log
                string contenido =
                    $"========================================{Environment.NewLine}" +
                    $"Código:    {codigo}{Environment.NewLine}" +
                    $"Fecha:     {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
                    $"Usuario:   {usuario}{Environment.NewLine}" +
                    $"URL:       {url}{Environment.NewLine}" +
                    $"Mensaje:   {ex.Message}{Environment.NewLine}" +
                    $"Tipo:      {ex.GetType().FullName}{Environment.NewLine}" +
                    $"Stack:     {ex.StackTrace}{Environment.NewLine}";

                if (ex.InnerException != null)
                {
                    contenido += $"Inner:     {ex.InnerException.Message}{Environment.NewLine}";
                }

                contenido += $"========================================{Environment.NewLine}{Environment.NewLine}";

                File.AppendAllText(archivo, contenido);
            }
            catch
            {
                // Si falla el log, no romper la app
                // (es preferible que el usuario vea el error 500 a que falle el manejo de errores)
            }
        }
    }
}