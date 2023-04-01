using Microsoft.Extensions.Logging;

namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Handler : IHandler
    {
        private readonly ILogger<Handler> _logger;

        public Handler(ILogger<Handler> logger)
        {
            _logger = logger;
        }

        public (bool success, object answer) Process(RemoteCommand command)
        {
            switch (command.Cmd)
            {
                case "info":
                {
                    var info = new
                    {
                        branchName = GitVersionInformation.BranchName,
                        sha = GitVersionInformation.Sha,
                        shortSha = GitVersionInformation.ShortSha,
                        semVer = GitVersionInformation.SemVer,
                        commitDate = GitVersionInformation.CommitDate
                    };

                    _logger.LogDebug("Info");

                    return (true, info);
                }
                default:
                    return (false, new object());
            }
        }

        public void Dispose()
        {
        }
    }
}