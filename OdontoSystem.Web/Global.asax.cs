using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace OdontoSystem.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // HU-14: esta versión de Rotativa NO tiene una clase RotativaConfiguration.
            // No hace falta configurar nada aquí: si ActionAsPdf.WkhtmlPath queda vacío,
            // la librería lo resuelve sola a Server.MapPath("~/Rotativa") — la carpeta
            // que el propio paquete NuGet ya copió con wkhtmltopdf.exe adentro.
        }
    }
}
