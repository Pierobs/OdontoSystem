using System.Web.Mvc;
using System.Web.Routing;
using OdontoSystem.BLL.Services;

namespace OdontoSystem.Web.Filters
{
    public class RequiereCambioPasswordAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session == null || session["IdUsuario"] == null)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            string controlador = filterContext.RouteData.Values["controller"]?.ToString()?.ToLower();
            string accion = filterContext.RouteData.Values["action"]?.ToString()?.ToLower();

            // Rutas permitidas siempre (sin bloqueo) — evita loop infinito
            bool esRutaPermitida =
                controlador == "account" ||
                controlador == "error";

            if (esRutaPermitida)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            int idUsuario = (int)session["IdUsuario"];
            var service = new UsuarioService();

            if (service.RequiereCambioPassword(idUsuario))
            {
                filterContext.Controller.TempData["AvisoPassword"] =
                    "🔒 Por seguridad, debes cambiar tu contraseña antes de continuar.";

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "CambiarMiPassword" }
                    });
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}