using System.Web.Mvc;
using System.Web.Routing;

namespace OdontoSystem.Web.Filters
{
    /// <summary>
    /// Solo permite acceso a usuarios con rol 'Administrador'.
    /// </summary>
    public class SoloAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var rol = filterContext.HttpContext.Session["Rol"]?.ToString();

            if (filterContext.HttpContext.Session["IdUsuario"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                return;
            }

            if (rol != "Administrador")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "AccesoDenegado" }));
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}