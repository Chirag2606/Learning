using Cyara.Web.Portal;

using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Cyara.Web.Portal
{
    using System.Web;
    using System.Web.Mvc;

    using Autofac;
    using Cyara.Foundation.Core.Settings;
    using Cyara.Redis.Core.Types;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Identity;
    using Cyara.Shared.Web.Identity.ActiveDirectory;
    using Cyara.Shared.Web.Settings;
    using Cyara.Web.Portal.Core.Mvc;
    using Cyara.Web.Portal.Hubs;

    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Infrastructure;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.DataProtection;

    using Owin;

    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            GlobalHost.HubPipeline.AddModule(new SignalrErrorHandler());

            var settings = DependencyResolver.Current.GetService<WebSiteSettings>();
            var redisSettings = DependencyResolver.Current.GetService<UpdateableConfig<RedisSettings>>();

            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = settings.EnableSignalRDetailedErrors };

            var builder = new ContainerBuilder();
            builder.RegisterInstance(hubConfiguration.Resolver.Resolve<IConnectionManager>());
            builder.RegisterType<TestCaseHub>().ExternallyOwned();

            builder.RegisterType<CyaraUserStore>().AsSelf().As<ICyaraUserStore>().InstancePerLifetimeScope();
            LoginProviders.ActiveDirectory.RegisterType<ADLoginProvider>(builder).InstancePerLifetimeScope();
            LoginProviders.Identity.RegisterType<IdentityLoginProvider>(builder).InstancePerLifetimeScope();
            LoginProviders.Saml.RegisterType<SamlLoginProvider>(builder).InstancePerLifetimeScope();
            LoginProviders.Google.RegisterType<GoogleLoginProvider>(builder).InstancePerLifetimeScope();
            builder.RegisterType<CyaraUserManager>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<CyaraSignInManager>().AsSelf().InstancePerLifetimeScope();
            builder.Register<IOwinContext>(c => HttpContext.Current.GetOwinContext()).InstancePerLifetimeScope();
            builder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication).InstancePerLifetimeScope();
            builder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider()).InstancePerRequest();

            builder.Update(MvcApplication.Container);

            // REGISTER WITH OWIN
            app.UseAutofacMiddleware(MvcApplication.Container);
            app.UseAutofacMvc();

            ConfigureAuth(app);

            // check if redis settings is enabled , if yes connect to redis backplane.
            if (redisSettings.Settings.Enabled)
            {
                var connectionString = $"{redisSettings.Settings.Host}:{redisSettings.Settings.Port},password={redisSettings.Settings.AccessKey},ssl={redisSettings.Settings.Ssl}";
                GlobalHost.DependencyResolver.UseStackExchangeRedis(new RedisScaleoutConfiguration(connectionString, Resources.Common.PortalSignalREventKey));
            }

            app.MapSignalR(hubConfiguration);
        }
    }
}