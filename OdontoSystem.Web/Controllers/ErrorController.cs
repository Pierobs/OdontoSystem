using System;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            return View("~/Views/Shared/Error/Error404.cshtml");
        }

        public ActionResult Error500(string codigo = null)
        {
            Response.StatusCode = 500;
            ViewBag.CodigoError = codigo;
            return View("~/Views/Shared/Error/Error500.cshtml");
        }

        public ActionResult AccesoDenegado()
        {
            Response.StatusCode = 403;
            return View("~/Views/Shared/Error/AccesoDenegado.cshtml");
        }
    }
}