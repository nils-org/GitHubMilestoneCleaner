﻿using GitHubMilestoneCleaner.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubMilestoneCleaner;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(c =>
        {
            c.SetExceptionHandler(ex =>
                {
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                    return -99;
                })
                .AddCommand<CleanVersionBumpsCommand>("version-bumps")
                .WithAlias("versionbumps")
                .WithDescription(
                    "Cleans multiple version bumps per library as are created by dependabot or renovate.");
#if DEBUG
            c.ValidateExamples();
#endif
        });
        return app.Run(args);
    }
}