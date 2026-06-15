using System.Web;
using System.Web.Mvc;

namespace OdontoSystem.Web.Filters
{
    public class SoloOdontologoOAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = HttpContext.Current.Session;

            if (session["IdUsuario"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login"
                    }));
                return;
            }

            string rol = session["Rol"]?.ToString();

            if (rol != "Odontologo" && rol != "Administrador")
            {
                filterContext.Controller.TempData["Error"] =
                    "⛔ Acceso denegado. Solo el odontólogo o el administrador pueden atender citas.";

                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new
                    {
                        controller = "Citas",
                        action = "Index"
                    }));
            }

            base.OnActionExecuting(filterContext);
        }
    }
}