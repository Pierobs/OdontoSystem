using System.Web.Mvc;
using System.Web.Routing;
using OdontoSystem.BLL.Services;

namespace OdontoSystem.Web.Filters
{
    public class RequiereCambioPasswordAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 1. Si no hay sesión iniciada, no hacemos nada
            var session = filterContext.HttpContext.Session;
            if (session == null || session["IdUsuario"] == null)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // 2. Obtener qué controlador y acción se está ejecutando
            string controlador = filterContext.RouteData.Values["controller"]?.ToString()?.ToLower();
            string accion = filterContext.RouteData.Values["action"]?.ToString()?.ToLower();

            // 3. Lista de rutas que SIEMPRE deben funcionar (sin bloqueo)
            // - Account: login, logout, cambiar contraseña obligatorio
            // - Error: 404, 500, acceso denegado
            bool esRutaPermitida =
                controlador == "account" ||
                controlador == "error";

            if (esRutaPermitida)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // 4. Verificar si este usuario debe cambiar su contraseña
            int idUsuario = (int)session["IdUsuario"];
            var service = new UsuarioService();

            if (service.RequiereCambioPassword(idUsuario))
            {
                // Guardar mensaje en TempData para mostrar en la pantalla destino
                filterContext.Controller.TempData["AvisoPassword"] =
                    "🔒 Por seguridad, debes cambiar tu contraseña antes de continuar.";

                // Redirigir a la pantalla de cambio obligatorio
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "CambiarMiPassword" }
                    });
                return;
            }

            // 5. Todo OK, deja pasar
            base.OnActionExecuting(filterContext);
        }
    }
}