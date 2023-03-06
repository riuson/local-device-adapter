using Autofac;
using Ninja.WebSockets;

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
            builder.RegisterType<WebServer>()
                .AsImplementedInterfaces()
                .AsSelf();
            builder.RegisterType<WebSocketServerFactory>()
                .AsImplementedInterfaces()
                .AsSelf();
        }
    }
}