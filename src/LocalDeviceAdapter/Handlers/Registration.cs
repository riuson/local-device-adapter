using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace LocalDeviceAdapter.Handlers
{
    public class Registration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(x => x.IsAssignableTo<IHandlerInitializer>() && !x.IsInterface)
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(x => x.IsAssignableTo<IHandler>() && !x.IsInterface)
                .AsSelf()
                .AsImplementedInterfaces();
        }
    }
}