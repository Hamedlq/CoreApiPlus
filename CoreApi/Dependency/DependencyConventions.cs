using System.Web.Mvc;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System.Web.Http.Controllers;
using CoreManager.AdminManager;
using CoreManager.DiscountManager;
using CoreManager.DriverManager;
using CoreManager.FanapManager;
using CoreManager.GroupManager;
using CoreManager.LogProvider;
using CoreManager.NotificationManager;
using CoreManager.PaymentManager;
using CoreManager.PricingManager;
using CoreManager.ResponseProvider;
using CoreManager.RouteGroupManager;
using CoreManager.RouteManager;
using CoreManager.TaxiMeterManager;
using CoreManager.TimingService;
using CoreManager.TransactionManager;
using CoreManager.UserManager;

namespace CoreApi.Dependency
{
    public class DependencyConventions : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(AllTypes.FromThisAssembly()
                                .BasedOn<IHttpController>()
                                .LifestyleTransient());

            container.Register(

                        Component.For<IPricingManager, PricingManager>().ImplementedBy<PricingManager>(),

                        Component.For<ITimingStrategy, TimingStrategy>().ImplementedBy<TimingStrategy>(),

                        Component.For<ITimingService, TimingService>().ImplementedBy<TimingService>(),

                        Component.For<INotificationManager, NotificationManager>().ImplementedBy<NotificationManager>(),

                        Component.For<ITransactionManager, TransactionManager>().ImplementedBy<TransactionManager>(),

                        Component.For<IDiscountManager, DiscountManager>().ImplementedBy<DiscountManager>(),

                        Component.For<IFanapManager, FanapManager>().ImplementedBy<FanapManager>(),

                        Component.For<IDriverManager, DriverManager>().ImplementedBy<DriverManager>(),

                        Component.For<ITaxiMeterManager, TaxiMeterManager>().ImplementedBy<TaxiMeterManager>(),

                        //Component.For<IResponseProvider>().ImplementedBy<ResponseProvider>(),

                        Component.For<ILogProvider>().ImplementedBy<LogProvider>()

                        //                        AllTypes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient()

                        //                        AllTypes.FromThisAssembly().BasedOn<IResponseProvider>().LifestyleTransient()
                        )
                       .AddFacility<LoggingFacility>(f => f.UseLog4Net());

            container.Register(

                Component.For<IRouteManager>().ImplementedBy<RouteManager>().LifestylePerWebRequest(),

                Component.For<IAdminManager>().ImplementedBy<AdminManager>().LifestylePerWebRequest(),

                Component.For<IGroupManager>().ImplementedBy<GroupManager>().LifestylePerWebRequest(),

                Component.For<IUserManager>().ImplementedBy<UserManager>().LifestylePerWebRequest(),

                Component.For<IRouteGroupManager>().ImplementedBy<RouteGroupManager>().LifestylePerWebRequest(),

                Component.For<IPaymentManager>().ImplementedBy<PaymentManager>().LifestylePerWebRequest(),

                Component.For<IResponseProvider>().ImplementedBy<ResponseProvider>().LifestylePerWebRequest()
                );
            //LoggerFactory.SetCurrent(new TraceSourceLogFactory());
            //EntityValidatorFactory.SetCurrent(new DataAnnotationsEntityValidatorFactory());


        }

    }
}