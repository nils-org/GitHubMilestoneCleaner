using GitHubMilestoneCleaner.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(c =>
{
    c.SetExceptionHandler((ex, _) =>
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -99;
        });
    c.AddCommand<CleanVersionBumpsCommand>("version-bumps")
        .WithAlias("versionbumps")
        .WithDescription(
            "Cleans multiple version bumps per library, as they are created by dependabot or renovate.");
    c.AddCommand<MoveContentsCommand>("move")
        .WithDescription("Moves all issues/PRs between milestones.");
#if DEBUG
    c.ValidateExamples();
#endif
});
return app.Run(args);