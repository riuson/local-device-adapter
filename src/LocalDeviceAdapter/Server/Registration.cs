using Autofac;

namespace LocalDeviceAdapter.Server
{
    public class Registration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Server>()
                .AsImplementedInterfaces();
            builder.RegisterType<ServerOptions>()
                .AsImplementedInterfaces();
        }
    }
}