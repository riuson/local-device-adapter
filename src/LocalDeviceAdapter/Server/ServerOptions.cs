using System.Net;

namespace LocalDeviceAdapter.Server
{
    internal class ServerOptions : IServerOptions
    {
        public int LowerPort { get; set; } = 4649;
        public int UpperPort { get; set; } = 4659;
        public IPAddress IP { get; set; } = IPAddress.Loopback;
    }
}