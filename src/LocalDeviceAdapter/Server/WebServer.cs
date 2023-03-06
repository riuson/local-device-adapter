﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ninja.WebSockets;

namespace LocalDeviceAdapter.Server
{
    public class WebServer : IDisposable
    {
        // const int BUFFER_SIZE = 1 * 1024 * 1024 * 1024; // 1GB
        private const int BUFFER_SIZE = 4 * 1024 * 1024; // 4MB
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly HashSet<string> _supportedSubProtocols;
        private readonly IWebSocketServerFactory _webSocketServerFactory;
        private bool _isDisposed;
        private TcpListener _listener;

        public WebServer(
            IWebSocketServerFactory webSocketServerFactory,
            ILoggerFactory loggerFactory,
            IList<string> supportedSubProtocols = null)
        {
            _logger = loggerFactory.CreateLogger<WebServer>();
            _webSocketServerFactory = webSocketServerFactory;
            _loggerFactory = loggerFactory;
            _supportedSubProtocols = new HashSet<string>(supportedSubProtocols ?? new string[0]);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                // safely attempt to shut down the listener
                try
                {
                    if (_listener != null)
                    {
                        if (_listener.Server != null) _listener.Server.Close();

                        _listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                _logger.LogInformation("Web Server disposed");
            }
        }

        private void ProcessTcpClient(TcpClient tcpClient)
        {
            Task.Run(() => ProcessTcpClientAsync(tcpClient));
        }

        private string GetSubProtocol(IList<string> requestedSubProtocols)
        {
            foreach (var subProtocol in requestedSubProtocols)
                // match the first sub protocol that we support (the client should pass the most preferable sub protocols first)
                if (_supportedSubProtocols.Contains(subProtocol))
                {
                    _logger.LogInformation($"Http header has requested sub protocol {subProtocol} which is supported");

                    return subProtocol;
                }

            if (requestedSubProtocols.Count > 0)
                _logger.LogWarning(
                    $"Http header has requested the following sub protocols: {string.Join(", ", requestedSubProtocols)}. There are no supported protocols configured that match.");

            return null;
        }

        private async Task ProcessTcpClientAsync(TcpClient tcpClient)
        {
            var source = new CancellationTokenSource();

            try
            {
                if (_isDisposed) return;

                // this worker thread stays alive until either of the following happens:
                // Client sends a close conection request OR
                // An unhandled exception is thrown OR
                // The server is disposed
                _logger.LogInformation("Server: Connection opened. Reading Http header from stream");

                // get a secure or insecure stream
                Stream stream = tcpClient.GetStream();
                var context = await _webSocketServerFactory.ReadHttpHeaderFromStreamAsync(stream);
                if (context.IsWebSocketRequest)
                {
                    var subProtocol = GetSubProtocol(context.WebSocketRequestedProtocols);
                    var options = new WebSocketServerOptions
                        { KeepAliveInterval = TimeSpan.FromSeconds(30), SubProtocol = subProtocol };
                    _logger.LogInformation(
                        "Http header has requested an upgrade to Web Socket protocol. Negotiating Web Socket handshake");

                    var webSocket = await _webSocketServerFactory.AcceptWebSocketAsync(context, options);

                    _logger.LogInformation("Web Socket handshake response sent. Stream ready.");
                    await RespondToWebSocketRequestAsync(webSocket, source.Token);
                }
                else
                {
                    _logger.LogInformation("Http header contains no web socket upgrade request. Ignoring");
                }

                _logger.LogInformation("Server: Connection closed");
            }
            catch (ObjectDisposedException)
            {
                // do nothing. This will be thrown if the Listener has been stopped
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
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
                    _logger.LogError($"Failed to close TCP connection: {ex}");
                }
            }
        }

        public async Task RespondToWebSocketRequestAsync(WebSocket webSocket, CancellationToken token)
        {
            var buffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);

            while (true)
            {
                var result = await webSocket.ReceiveAsync(buffer, token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation(
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

                // just echo the message back to the client
                var toSend = new ArraySegment<byte>(buffer.Array, buffer.Offset, result.Count);
                //await webSocket.SendAsync(toSend, WebSocketMessageType.Binary, true, token);
                var segment = new ArraySegment<byte>(Encoding.ASCII.GetBytes("Hello!"));
                await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, token);
            }
        }

        public async Task Listen(int port)
        {
            try
            {
                var localAddress = IPAddress.Any;
                _listener = new TcpListener(localAddress, port);
                _listener.Start();
                _logger.LogInformation($"Server started listening on port {port}");
                while (true)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    ProcessTcpClient(tcpClient);
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
    }
}