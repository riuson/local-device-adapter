using System;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Handler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsText)
                try
                {
                    var command = JsonSerializer.Deserialize<RemoteCommand>(e.Data);
                    if (command is null)
                    {
                        Send("Invalid command");
                        return;
                    }

                    switch (command.cmd)
                    {
                        case "info":
                        {
                            var json = GetAdapterInfo();
                            Send(json);
                            return;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Send("Invalid command");
                    return;
                }

            Send("Invalid command");
        }

        private static string GetAdapterInfo()
        {
            var info = new
            {
                branchName = GitVersionInformation.BranchName,
                sha = GitVersionInformation.Sha,
                shortSha = GitVersionInformation.ShortSha,
                semVer = GitVersionInformation.SemVer,
                commitDate = GitVersionInformation.CommitDate
            };
            var json = JsonSerializer.Serialize(
                info,
                new JsonSerializerOptions
                {
                    AllowTrailingCommas = false
                });
            return json;
        }
    }
}