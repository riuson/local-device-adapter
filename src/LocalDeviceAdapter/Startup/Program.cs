using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocalDeviceAdapter
{
    internal static class Program
    {
        private static IContainer CompositionRoot()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Application>();
            containerBuilder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            containerBuilder.RegisterType<LoggerFactory>()
                .As<ILoggerFactory>()
                .SingleInstance();
            containerBuilder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(loggingbuilder => loggingbuilder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug));

            containerBuilder.Populate(serviceCollection);

            return containerBuilder.Build();
        }

        public static void Main() //Main entry point
        {
            var container = CompositionRoot();
            var app = container.Resolve<Application>();
            app.Run();
        }
    }
}