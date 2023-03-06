namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Handler : IHandler
    {
        public (bool success, object answer) Process(RemoteCommand command)
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