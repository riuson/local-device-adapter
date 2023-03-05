namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Handler : HandlerBase
    {
        protected override (bool success, object answer) ProcessCommand(RemoteCommand command)
        {
            switch (command.cmd)
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

                    return (true, info);
                }
                default:
                    return (false, new object());
            }
        }
    }
}