using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LocalDeviceAdapter.Handlers
{
    public class RemoteCommand
    {
        [JsonInclude]
        [JsonPropertyName("cmd")]
        public string Cmd { get; set; }

        [JsonInclude]
        [JsonPropertyName("args")]
        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
    }
}