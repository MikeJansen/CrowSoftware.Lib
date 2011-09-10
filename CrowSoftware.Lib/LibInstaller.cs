using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Logging.NLogIntegration;
using Castle.Windsor;
using CrowSoftware.Common.Config;
using CrowSoftware.Common.Log;
using CrowSoftware.Lib.Net;

namespace CrowSoftware.Common
{
    public class LibInstaller: IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            RegisterLogging(container);
            RegisterConfig(container);
        }

        public static void RegisterConfig(IWindsorContainer container)
        {
            container.Register(Component.For<IConfigManager>().ImplementedBy<ConfigManager>());
            container.Register(Component.For<IAsyncTcpClient>().ImplementedBy<AsyncTcpClient>());
        }

        public static void RegisterLogging(IWindsorContainer container)
        {
            container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.NLog).WithConfig("NLog.config"));
            container.Register(Component.For<ILoggerFactory>().ImplementedBy<NLogFactory>());
            container.Register(Component.For<ILogManager>().ImplementedBy<LogManager>());
        }
    }
}
