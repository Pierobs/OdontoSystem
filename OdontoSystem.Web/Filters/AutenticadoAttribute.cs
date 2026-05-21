using System.Web.Mvc;
using System.Web.Routing;

namespace OdontoSystem.Web.Filters
{
    /// <summary>
    /// Bloquea el acceso a páginas si el usuario no inició sesión.
    /// </summary>
    public class AutenticadoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["IdUsuario"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login"
                    }));
            }
            base.OnActionExecuting(filterContext);
        }
    }
}