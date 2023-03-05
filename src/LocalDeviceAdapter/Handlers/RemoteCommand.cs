using System.Collections.Generic;

namespace LocalDeviceAdapter.Handlers
{
    internal class RemoteCommand
    {
        public string cmd { get; set; }
        public Dictionary<string, string> args { get; } = new Dictionary<string, string>();
    }
}