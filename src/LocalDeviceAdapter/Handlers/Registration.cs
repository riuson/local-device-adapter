using Autofac;
using System.Reflection;
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
        }
    }
}