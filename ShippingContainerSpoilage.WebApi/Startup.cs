using System.Configuration;
using System.Web.Http;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ShippingContainerSpoilage.WebApi.Startup))]

namespace ShippingContainerSpoilage.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();
            AutofacConfig.Configure(configuration);
            app.UseAutofacMiddleware(AutofacConfig.Container);
            RouteConfig.Configure(configuration);
            app.UseWebApi(configuration);
            SwaggerConfig.Configure(configuration);
        }
    }
}