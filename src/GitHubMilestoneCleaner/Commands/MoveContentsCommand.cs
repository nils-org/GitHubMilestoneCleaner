using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GitHubMilestoneCleaner.Engines;
using GitHubMilestoneCleaner.Extension;
using JetBrains.Annotations;
using Octokit;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubMilestoneCleaner.Commands;

[UsedImplicitly]
internal sealed class MoveContentsCommand : AsyncCommand<MoveContentsCommand.Settings>
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class Settings : CommonCommandSettings
    {
        [Description("Interactively select which issues to remove from the milestone.")]
        [CommandOption("-q|--non-interactive")]
        public bool NonInteractive { get; set; }

        [Description("Do not actually remove any items, only show what would be removed.")]
        [CommandOption("-w|--whatIf")]
        public bool WhatIf { get; set; }
        
        [Description("Source Milestone.")]
        [CommandOption("-s|--source")] 
        public string Source { get; set; } = string.Empty;
        
        [Description("Destination Milestone.")]
        [CommandOption("-d|--destination")] 
        public string Destination { get; set; } = string.Empty;
    }
    
    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrEmpty(settings.Source))
        {
            return ValidationResult.Error("Source Milestone is required.");
        }
        if (string.IsNullOrEmpty(settings.Destination))
        {
            return ValidationResult.Error("Destination Milestone is required.");
        }
        
        return CommonCommandSettings.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var adapter = new GitHubAdapter(settings.Token);
        Repository repo;
        IEnumerable<Issue> issues;
        Milestone source;
        Milestone destination;
        try
        {
            repo = await adapter.GetRepository(settings.Owner, settings.Repository);
            source = await adapter.GetMilestone(repo, settings.Source, true);
            destination = await adapter.GetMilestone(repo, settings.Destination, true);
            issues = await adapter.GetIssuesInMileStone(repo, source);
        }
        catch (GitHubAdapter.ExecutionAbortedException e)
        {
            return e.Reason;
        }
        
        var toMove = new List<Issue>();
        if (!settings.NonInteractive)
        {
            var prompt = new MultiSelectionPrompt<Issue>
            {
                Converter = IssueExtensions.ToMarkup,
                PageSize = 25,
                Title = $"Select issues to to move from milestone {source.Title} to {destination.Title}",
                Mode = SelectionMode.Independent,
            };
            issues
                .OrderBy(i => i.Number)
                .ToList()
                .ForEach(i =>
                {
                    prompt
                        .AddChoice(i)
                        .Select();
                });
            toMove = AnsiConsole.Prompt(prompt);
        }
        else
        {
            toMove.AddRange(issues);
            var table = new Table();
            table.AddColumn(new TableColumn("Number").RightAligned());
            table.AddColumn("Name");
            toMove.ForEach(x => table.AddRow(x.Number.ToString(), x.Title));
            AnsiConsole.Write(table);
        }
        
        if (!toMove.Any())
        {
            return 0;
        }

        AnsiConsole.MarkupLine($"[orange3]Moving the following issues: {toMove.ToShortMarkup()}[/]");
        if (settings.WhatIf)
        {
            return 0;
        }
        
        async Task DoMove(Action? callback = null)
        {
            foreach (var issue in toMove)
            {
                await adapter.MoveToMilestone(repo, issue, destination);
                callback?.Invoke();
            }
        }
        
        if (settings.NonInteractive)
        {
            await DoMove();
        }
        else
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var inc = 100d / toMove.Count;
                    var task = ctx.AddTask($"Moving {toMove.Count} issues");
                    await DoMove(() =>
                    {
                        task.Increment(inc);
                    });

                    var rest = 100 - task.Percentage;
                    task.Increment(rest);
                });
        }

        return 0;
    }
}