using Autofac;
using System.Reflection;

namespace LocalDeviceAdapter
{
    internal static class Program
    {
        private static IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Application>();
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
            return builder.Build();
        }

        public static void Main() //Main entry point
        {
            var container = CompositionRoot();
            var app = container.Resolve<Application>();
            app.Run();
        }
    }
}