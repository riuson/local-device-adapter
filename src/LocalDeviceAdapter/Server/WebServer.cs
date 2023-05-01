﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LocalDeviceAdapter.Handlers;
using Microsoft.Extensions.Logging;
using Ninja.WebSockets;

namespace LocalDeviceAdapter.Server
{
    public class WebServer : IDisposable
    {
        // const int BUFFER_SIZE = 1 * 1024 * 1024 * 1024; // 1GB
        private const int BUFFER_SIZE = 4 * 1024 * 1024; // 4MB
        private readonly Dictionary<string, IHandlerInitializer> _handlerInitializers;
        private readonly ILogger<WebServer> _logger;
        private readonly IWebSocketServerFactory _webSocketServerFactory;
        private bool _isDisposed;
        private TcpListener _listener;

        public WebServer(
            ILogger<WebServer> logger,
            IWebSocketServerFactory webSocketServerFactory,
            IEnumerable<IHandlerInitializer> handlerInitializers)
        {
            this._logger = logger;
            this._webSocketServerFactory = webSocketServerFactory;
            this._handlerInitializers = handlerInitializers
                .ToDictionary(x => x.Path);
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                this._isDisposed = true;

                // safely attempt to shut down the listener
                try
                {
                    this._listener?.Server?.Close();
                    this._listener?.Stop();
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex.ToString());
                }

                this._logger.LogInformation("Web Server disposed");
            }
        }

        public async Task Listen(IPAddress ipAddress, int port)
        {
            try
            {
                this._listener = new TcpListener(ipAddress, port);
                this._listener.Start();
                this._logger.LogInformation($"Server started listening on port {port}");

                while (true)
                {
                    var tcpClient = await this._listener.AcceptTcpClientAsync();
                    var taskClient = this.ProcessTcpClientAsync(tcpClient);
                }
            }
            catch (SocketException ex)
            {
                var message =
                    string.Format(
                        "Error listening on port {0}. Make sure IIS or another application is not running and consuming your port.",
                        port);
                throw new Exception(message, ex);
            }
        }

        private async Task ProcessTcpClientAsync(TcpClient tcpClient)
        {
            var source = new CancellationTokenSource();

            try
            {
                if (this._isDisposed) return;

                // this worker thread stays alive until either of the following happens:
                // Client sends a close conection request OR
                // An unhandled exception is thrown OR
                // The server is disposed
                this._logger.LogInformation("Server: Connection opened. Reading Http header from stream");

                // get a secure or insecure stream
                Stream stream = tcpClient.GetStream();
                var context = await this._webSocketServerFactory.ReadHttpHeaderFromStreamAsync(stream);

                if (context.IsWebSocketRequest)
                {
                    var header = context.HttpHeader;
                    var path = GetPath(header);
                    var origin = GetOrigin(header);

                    if (this._handlerInitializers.ContainsKey(path))
                    {
                        var options = new WebSocketServerOptions
                        {
                            KeepAliveInterval = TimeSpan.FromSeconds(30)
                        };
                        this._logger.LogInformation(
                            "Http header has requested an upgrade to Web Socket protocol. Negotiating Web Socket handshake.");

                        var webSocket = await this._webSocketServerFactory.AcceptWebSocketAsync(context, options);

                        this._logger.LogInformation("Web Socket handshake response sent. Stream ready.");
                        await this.RespondToWebSocketRequestAsync(
                            webSocket,
                            source.Token,
                            path,
                            origin,
                            this._handlerInitializers[path]);
                    }
                    else
                    {
                        this._logger.LogInformation("No handlers are expecting for this path. Ignoring.");
                    }
                }
                else
                {
                    this._logger.LogInformation("Http header contains no web socket upgrade request. Ignoring.");
                }

                this._logger.LogInformation("Server: Connection closed.");
            }
            catch (ObjectDisposedException)
            {
                // do nothing. This will be thrown if the Listener has been stopped
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
            }
            finally
            {
                try
                {
                    tcpClient.Client.Close();
                    tcpClient.Close();
                    source.Cancel();
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"Failed to close TCP connection: {ex}");
                }
            }
        }

        public async Task RespondToWebSocketRequestAsync(
            WebSocket webSocket,
            CancellationToken token,
            string path,
            string origin,
            IHandlerInitializer handlerInitializer)
        {
            var buffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);

            using (var handler = handlerInitializer.CreateHandler())
            {
                while (true)
                {
                    var result = await webSocket.ReceiveAsync(buffer, token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        this._logger.LogInformation(
                            $"Client initiated close. Status: {result.CloseStatus} Description: {result.CloseStatusDescription}");
                        break;
                    }

                    if (result.Count > BUFFER_SIZE)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig,
                            $"Web socket frame cannot exceed buffer size of {BUFFER_SIZE:#,##0} bytes. Send multiple frames instead.",
                            token);
                        break;
                    }

                    var answerObject = this.HandleRequest(
                        handler,
                        new ArraySegment<byte>(buffer.Array, buffer.Offset, result.Count));
                    var answerJson = JsonSerializer.Serialize(answerObject);
                    var answerArray = new ArraySegment<byte>(Encoding.ASCII.GetBytes(answerJson));
                    await webSocket.SendAsync(
                        answerArray,
                        WebSocketMessageType.Text,
                        true,
                        token);
                }
            }
        }

        private object HandleRequest(IHandler handler, ArraySegment<byte> requestArray)
        {
            try
            {
                var requestJson = ArrayToString(requestArray);
                var request = JsonSerializer.Deserialize<RemoteCommand>(requestJson);
                var answer = handler.Process(request);

                return new
                {
                    status = answer.success ? "success" : "failed",
                    data = answer.answer
                };
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Exception occur while processing request.");

                return new
                {
                    status = "failed",
                    error = "An unhandled exception was occur while processing request.",
#if DEBUG
                    message = e.Message,
                    stack = e.StackTrace,
#endif
                };
            }
        }

        private static string GetPath(string httpHeader)
        {
            //GET /Echo HTTP/1.1
            var regex = new Regex(@"(?<=^GET\s+)\S+?(?=\s+HTTP)", RegexOptions.IgnoreCase);
            var match = regex.Match(httpHeader);

            if (!match.Success) return string.Empty;

            return match.Value;
        }

        private static string GetOrigin(string httpHeader)
        {
            //Origin: https://www.example.com
            var regex = new Regex(@"(?<=Origin:\s+)\S+", RegexOptions.IgnoreCase);
            var match = regex.Match(httpHeader);

            if (!match.Success) return string.Empty;

            return match.Value;
        }

        private static string ArrayToString(ArraySegment<byte> array)
        {
            return Encoding.ASCII.GetString(array.ToArray());
        }
    }
}
