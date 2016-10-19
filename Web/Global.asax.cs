using AzureScaleLeetTreats.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AzureScaleLeetTreats
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ElasticDbConfiguration.SetConfiguration(new ElasticDbConfiguration());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
