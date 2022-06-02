namespace Cyara.Web.Portal
{
    using System.Web.Http;

    using Autofac;
    using Autofac.Integration.WebApi;

    using Cyara.Shared.FileTransfer.Server.Contracts;

    using Owin;

    public class DirectFileTransferWebApiStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            var owinStartup = MvcApplication.Container.Resolve<IOwinStartup>();

            owinStartup.Configuration(appBuilder, MvcApplication.Container);

            config.MapHttpAttributeRoutes();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(MvcApplication.Container);

            appBuilder.UseWebApi(config);
        }
    }
}