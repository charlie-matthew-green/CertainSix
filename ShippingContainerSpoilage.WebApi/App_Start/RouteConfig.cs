using System;
using System.Web.Http;
using Swashbuckle.Application;

namespace ShippingContainerSpoilage.WebApi
{
    public static class RouteConfig
    {
        public static void Configure(HttpConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            configuration.MapHttpAttributeRoutes();

            using (var handler = new RedirectHandler(m => m.RequestUri.ToString(), "swagger"))
            {
                configuration.Routes.MapHttpRoute(
                    name: "swagger_root",
                    routeTemplate: "",
                    defaults: null,
                    constraints: null,
                    handler: handler);
            }
        }
    }
}