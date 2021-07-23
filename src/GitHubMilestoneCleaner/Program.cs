using GitHubMilestoneCleaner.Commands;
using Spectre.Console.Cli;

namespace GitHubMilestoneCleaner
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(c =>
            {
                c.AddCommand<CleanVersionBumpsCommand>("version-bumps")
                    .WithAlias("versionbumps")
                    .WithDescription("Cleans multiple version bumps per library as are created by dependabot or renovate.");
#if DEBUG
                c.ValidateExamples();
                c.PropagateExceptions();
#endif
            });
            return app.Run(args);
        }
    }
}
