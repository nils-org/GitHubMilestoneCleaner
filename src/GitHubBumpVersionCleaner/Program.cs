using GitHubBumpVersionCleaner.Commands;
using Spectre.Console.Cli;

namespace GitHubBumpVersionCleaner
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(c =>
            {
                c.AddCommand<ListCommand>("list")
                    .WithDescription("List issues that could be removed from the milestone.");
                c.AddCommand<InteractiveCommand>("interactive")
                    .WithDescription("Interactively list and select issues to be removed from the milestone.")
                    .WithExample(new []{"interactive", "-o <owner>", "-r <repo>", "-t <PAT>", "-m <milestone>"});
                c.AddCommand<AutoCleanupCommand>("autoclean")
                    .WithAlias("auto-clean")
                    .WithDescription("Automatically clean issues in the milestone.");
#if DEBUG
                c.ValidateExamples();
                c.PropagateExceptions();
#endif
            });
            return app.Run(args);
        }
    }
}
