using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LocalDeviceAdapter.Handlers.Uart
{
    internal class Handler : IHandler
    {
        protected Dictionary<string, SerialPort> _ports = new Dictionary<string, SerialPort>();

        public void Dispose()
        {
            foreach (var kvp in this._ports)
            {
                if (kvp.Value.IsOpen)
                {
                    kvp.Value.Close();
                    kvp.Value.Dispose();
                }
            }
        }

        public virtual (bool success, object answer) Process(RemoteCommand command)
        {
            switch (command.cmd)
            {
                case "list":
                    {
                        var ports = GetSerialPorts();
                        return (true, ports);
                    }
                case "open":
                    {
                        var (isOpened, message) = HandlePortOpen(command.args);
                        return (isOpened, message);
                    }
                case "close":
                    {
                        var (isClosed, message) = HandlePortClose(command.args);
                        return (isClosed, message);
                    }
                case "exchange":
                    {
                        var (isSuccess, message) = HandlePortExchange(command.args);
                        return (isSuccess, message);
                    }
                default:
                    return (false, new object());
            }
        }

        private static object GetSerialPorts()
        {
            // SDRP_HARDWAREID: USB\VID_10C4&PID_EA60&REV_0100USB\VID_10C4&PID_EA60
            var regexVid = new Regex(@"(?<=VID_)[0-9a-fA-F]{4}");
            var regexPid = new Regex(@"(?<=PID_)[0-9a-fA-F]{4}");

            int getId(Regex regex, string value)
            {
                var match = regex.Match(value);

                if (!match.Success) return -1;

                if (int.TryParse(match.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
                    return result;

                return -1;
            }

            var ports = Win32SerialPortEnum.GetAllCOMPorts()
                .Select(x => new
                {
                    name = x.Name,
                    description = x.Description,
                    friendlyName = x.FriendlyName,
                    vendorId = getId(regexVid, x.HardwareId),
                    productId = getId(regexPid, x.HardwareId)
                })
                .ToArray();
            return ports;
        }

        private (bool success, string message) HandlePortOpen(Dictionary<string, string> arguments)
        {
            string strValue;
            var message = new StringBuilder();

            var name = string.Empty;
            var isNameValid = false;

            if (arguments.TryGetValue("name", out strValue))
            {
                var ports = Win32SerialPortEnum.GetAllCOMPorts()
                    .Select(x => x.Name)
                    .ToArray();

                if (ports.Contains(strValue))
                {
                    name = strValue;
                    isNameValid = true;
                }
                else
                {
                    message.AppendLine($"Port {name} was not found.");
                }
            }

            var baudRate = 0;
            var isBaudRateValid = false;

            if (arguments.TryGetValue("baudrate", out strValue))
            {
                if (int.TryParse(strValue, out var value))
                {
                    if (value >= 1_200 && value <= 3_000_000)
                    {
                        baudRate = value;
                        isBaudRateValid = true;
                    }
                    else
                    {
                        message.AppendLine($"Baudrate should be in range [1200, 3000000], but {baudRate} is specified.");
                    }
                }
                else
                {
                    message.AppendLine($"Can't parse baudrate value '{strValue}'.");
                }
            }

            var parity = Parity.None;
            var isParityValid = false;

            if (arguments.TryGetValue("parity", out strValue))
            {
                switch (strValue)
                {
                    case "none":
                        {
                            isParityValid = true;
                            break;
                        }
                    case "even":
                        {
                            parity = Parity.Even;
                            isParityValid = true;
                            break;
                        }
                    case "odd":
                        {
                            parity = Parity.Odd;
                            isParityValid = true;
                            break;
                        }
                    default:
                        {
                            message.AppendLine($"Unexpected parity value '{strValue}'.");
                            break;
                        }
                }
            }

            var stopBits = StopBits.None;
            var isStopBitsValid = false;

            if (arguments.TryGetValue("stopbits", out strValue))
            {
                switch (strValue)
                {
                    case "none":
                    case "0":
                        {
                            isStopBitsValid = true;
                            break;
                        }
                    case "1":
                    case "one":
                        {
                            stopBits = StopBits.One;
                            isStopBitsValid = true;
                            break;
                        }
                    case "1.5":
                        {
                            stopBits = StopBits.OnePointFive;
                            isStopBitsValid = true;
                            break;
                        }
                    case "2":
                    case "two":
                        {
                            stopBits = StopBits.Two;
                            isStopBitsValid = true;
                            break;
                        }
                    default:
                        {
                            message.AppendLine($"Unexpected stopbits value '{strValue}'.");
                            break;
                        }
                }
            }

            if (isNameValid && isBaudRateValid && isParityValid && isStopBitsValid)
            {
                try
                {
                    if (this._ports.TryGetValue(name, out var openedPort))
                    {
                        this._ports.Remove(name);
                        openedPort.Close();
                        openedPort.Dispose();
                    }

                    var port = new SerialPort();
                    port.PortName = name;
                    port.BaudRate = baudRate;
                    port.Parity = parity;
                    port.StopBits = stopBits;
                    port.Open();

                    if (port.IsOpen)
                    {
                        port.ReadTimeout = 1000;
                        this._ports.Add(name, port);
                        return (port.IsOpen, $"Port {name} is open.");
                    }

                    return (false, "Unknown failure.");
                }
                catch (Exception exc)
                {
                    return (false, exc.Message);
                }
            }

            return (false, message.ToString());
        }

        private (bool success, string message) HandlePortClose(Dictionary<string, string> arguments)
        {
            string strValue;
            var message = new StringBuilder();

            var name = string.Empty;

            if (arguments.TryGetValue("name", out strValue))
            {
                if (this._ports.TryGetValue(strValue, out var port))
                {
                    port.Close();
                    this._ports.Remove(strValue);
                    port.Dispose();
                    return (true, $"Port {name} is closed.");
                }
                else
                {
                    message.AppendLine($"Port {name} was not found.");
                }
            }

            return (false, message.ToString());
        }

        private (bool success, string message) HandlePortExchange(Dictionary<string, string> arguments)
        {
            string strValue;
            var message = new StringBuilder();

            var name = string.Empty;
            var isNameValid = false;

            if (arguments.TryGetValue("name", out strValue))
            {
                var ports = Win32SerialPortEnum.GetAllCOMPorts()
                    .Select(x => x.Name)
                    .ToArray();

                if (ports.Contains(strValue) && this._ports.ContainsKey(strValue))
                {
                    name = strValue;
                    isNameValid = true;
                }
                else
                {
                    message.AppendLine($"Port {name} was not found.");
                }
            }

            var sendArray = new byte[] { };
            var isSendArrayValid = false;

            if (arguments.TryGetValue("sendHexString", out strValue))
            {
                if (strValue.Length == 0)
                {
                    message.AppendLine("Can't send zero-length data.");
                }
                else if ((strValue.Length & 1) != 0)
                {
                    message.AppendLine("Invalid length of data. It was expected that the length of the string would be a multiple of two.");
                }
                else
                {
                    try
                    {
                        sendArray = Enumerable.Range(0, strValue.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(strValue.Substring(x, 2), 16))
                            .ToArray();
                        isSendArrayValid = true;
                    }
                    catch (Exception e)
                    {
                        message.AppendLine(e.Message);
                    }
                }
            }

            if (isNameValid && isSendArrayValid)
            {
                try
                {
                    if (this._ports.TryGetValue(name, out var port))
                    {
                        if (!port.IsOpen)
                        {
                            message.AppendLine($"Port {name} is closed.");
                        }
                        else
                        {
                            port.Write(sendArray, 0, sendArray.Length);
                            var receivedArray = this.Receive(port);
                            return (true, string.Join("", receivedArray.Select(x => x.ToString("X2"))));
                        }
                    }
                }
                catch (Exception exc)
                {
                    return (false, exc.Message);
                }
            }

            return (false, message.ToString());
        }

        private byte[] Receive(SerialPort port)
        {
            var result = new List<byte>();
            var buffer = new byte[100];
            var readed = 0;

            while (true)
            {
                try
                {
                    readed = port.Read(buffer, 0, 100);
                }
                catch (TimeoutException exc)
                {
                    readed = 0;
                }

                if (readed > 0)
                {
                    result.AddRange(buffer.Take(readed));
                }
                else
                {
                    break;
                }
            }

            return result.ToArray();
        }
    }
}