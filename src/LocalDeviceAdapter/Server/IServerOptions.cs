using System.Net;

namespace LocalDeviceAdapter.Server
{
    public interface IServerOptions
    {
        int LowerPort { get; set; }
        int UpperPort { get; set; }
        IPAddress IP { get; set; }
    }
}