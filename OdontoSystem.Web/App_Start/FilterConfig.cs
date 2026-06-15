using System.Web.Mvc;
using OdontoSystem.Web.Filters;

namespace OdontoSystem.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ManejadorErrorGlobalAttribute());
            filters.Add(new RequiereCambioPasswordAttribute());
        }
    }
}