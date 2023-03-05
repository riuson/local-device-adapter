using System;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers
{
    internal abstract class HandlerBase : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var response = string.Empty;

            if (e.IsText)
                try
                {
                    var command = JsonSerializer.Deserialize<RemoteCommand>(e.Data);

                    if (command != null)
                    {
                        var result = ProcessCommand(command);

                        if (result.success)
                        {
                            SendAnswer(result.answer);
                            return;
                        }
                    }
                }
                catch (Exception exception)
                {
#if DEBUG
                    SendError(
                        "A problem encountered while processing command.",
                        exception);
                    return;
#endif
                }

            SendError("Invalid command.");
        }

        protected abstract (bool success, object answer) ProcessCommand(RemoteCommand command);

        private string Serialize(object value)
        {
            return JsonSerializer.Serialize(
                value,
                new JsonSerializerOptions
                {
                    AllowTrailingCommas = false
                });
        }

        private void SendAnswer(object value)
        {
            Send(Serialize(value));
        }

        private void SendError(string message, Exception exception = null)
        {
            if (exception is null)
                Send(
                    Serialize(
                        new
                        {
                            error = message
                        }));
            else
                Send(
                    Serialize(
                        new
                        {
                            error = message,
                            message = exception.Message,
                            stackTrace = exception.StackTrace
                        }));
        }
    }
}