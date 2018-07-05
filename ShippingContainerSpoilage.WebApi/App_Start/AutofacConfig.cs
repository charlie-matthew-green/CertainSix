using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using ShippingContainerSpoilage.WebApi.Controllers;

namespace ShippingContainerSpoilage.WebApi
{
    public class AutofacConfig
    {
        protected internal static IContainer Container;

        public static void Configure(HttpConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var builder = new ContainerBuilder();
            builder.RegisterType<DalFacade>().As<IDalFacade>().WithParameter("connectionString", ConfigurationManager.ConnectionStrings["ContainerSpoilage"].ConnectionString);
            builder.RegisterType<ContainerSpoilage>().As<IContainerSpoilage>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            Container = builder.Build();
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
        }
    }
}