using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.Windsor;
using CoreApi.Dependency;
using CoreManager.ResponseProvider;

namespace CoreApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly IWindsorContainer _container;
        public IResponseProvider _responseProvider;
        public WebApiApplication()
        {
            this._container =
                new WindsorContainer().Install(new DependencyConventions());
        }

        public override void Dispose()
        {
            this._container.Dispose();
            base.Dispose();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(this._container));

        }
    }
}
