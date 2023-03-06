using System.Collections.Generic;

namespace LocalDeviceAdapter.Handlers
{
    public class RemoteCommand
    {
        public string cmd { get; set; }
        public Dictionary<string, string> args { get; } = new Dictionary<string, string>();
    }
}