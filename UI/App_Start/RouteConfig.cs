using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "WeChat",
                "WeChat/{action}/{id}",
                new { controller = "Index", action = "Index", id = UrlParameter.Optional },
                new string[] { "UI.Areas.WeChat.Controllers" }
              );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional },
				namespaces: new[] { "UI.Controllers" }
            );
        }
    }
}