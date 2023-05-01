using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using LocalDeviceAdapter.Common;

namespace LocalDeviceAdapter.PlatformSpecific
{
    /// <summary>
    /// Linux-specific code for Serial Ports enumeration.
    /// </summary>
    public static class SerialPortsEnum
    {
        /// <summary>
        /// Gets list of Serial Ports in JSON format.
        /// </summary>
        public static IEnumerable<PortInfo> GetPortsList()
        {
            var scriptOutput = GetScriptOutput();
            var ports = ParseOutput(scriptOutput);
            return ports;
        }

        private static string GetScriptOutput()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location),
                "ttylist.sh");

            var fileInfo = new FileInfo(path);

            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = "/bin/sh",
                Arguments = $"\"{fileInfo.FullName}\"",
                RedirectStandardOutput = true,
            };

            using (var process = Process.Start(startInfo))
            {
                var result = new StringBuilder();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    result.AppendLine(line);
                }

                process.WaitForExit();

                return result.ToString();
            }
        }

        private static IEnumerable<PortInfo> ParseOutput(string value)
        {
            // name:\t/dev/ttyUSB0
            // description:\tCP2102\x20USB\x20to\x20UART\x20Bridge\x20Controller
            // friendlyName:\tCP2102\x20USB\x20to\x20UART\x20Bridge\x20Controller (/dev/ttyUSB0)
            // vendorId:\t4292
            // productId:\t60000
            // name:\t/dev/ttyUSB1
            // ...
            var result = new List<PortInfo>();
            var info = new PortInfo();

            foreach (var line in value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = line.Split('\t');
                var fieldName = pair[0].Trim();
                var fieldValue = pair[1].Trim();

                switch (fieldName)
                {
                    case "name":
                        {
                            info = new PortInfo()
                            {
                                DeviceName = fieldValue
                            };
                            result.Add(info);
                            break;
                        }
                    case "description":
                        {
                            info.Description = fieldValue.Replace("\x20", " ");
                            break;
                        }
                    case "friendlyName":
                        {
                            info.FriendlyName = fieldValue.Replace("\x20", " ");
                            break;
                        }
                    case "vendorId":
                        {
                            info.VendorId = int.Parse(fieldValue);
                            break;
                        }
                    case "productId":
                        {
                            info.ProductId = int.Parse(fieldValue);
                            break;
                        }
                }
            }

            return result;
        }
    }
}
