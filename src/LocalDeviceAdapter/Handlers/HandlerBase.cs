using System;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers
{
    internal class HandlerBase : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var returnedAnswer = "Invalid command.";

            if (e.IsText)
                try
                {
                    var command = JsonSerializer.Deserialize<RemoteCommand>(e.Data);

                    if (command != null)
                    {
                        var result = ProcessCommand(command);

                        if (result.success)
                            returnedAnswer = JsonSerializer.Serialize(
                                result.answer,
                                new JsonSerializerOptions
                                {
                                    AllowTrailingCommas = false
                                });
                    }
                }
                catch (Exception exception)
                {
                    returnedAnswer = "A problem encountered while processing command.";
                }

            Send(returnedAnswer);
        }

        protected virtual (bool success, object answer) ProcessCommand(RemoteCommand command)
        {
            return (false, new object());
        }
    }
}