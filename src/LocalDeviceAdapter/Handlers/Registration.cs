using Autofac;
using LocalDeviceAdapter.Handlers.Echo;

namespace LocalDeviceAdapter.Handlers
{
    public class Registration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Initializer>()
                .As<IHandlerInitializer>();
        }
    }
}