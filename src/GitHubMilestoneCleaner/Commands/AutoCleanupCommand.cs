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
internal sealed class CleanVersionBumpsCommand : AsyncCommand<CleanVersionBumpsCommand.Settings>
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

        [Description("The comment to add to issues that are removed from the milestone, if the issue is not on the first level of the issue-tree.")]
        [CommandOption("-s|--subIssueComment")]
        [DefaultValue("Superseded by {0}")]
        public string? SubIssueComment { get; set; }

        [Description("The comment to add to issues that are removed from the milestone, if the issue is on the first level of the issue-tree.")]
        [CommandOption("-i|--topIssueComment")]
        public string? TopIssueComment { get; set; }
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return CommonCommandSettings.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var adapter = new GitHubAdapter(settings.Token);
        Repository repo;
        IEnumerable<Issue> issues;
        Milestone milestone;
        try
        {
            repo = await adapter.GetRepository(settings.Owner, settings.Repository);
            milestone = await adapter.GetMilestone(repo, settings.Milestone, settings.SearchClosedMilestones);
            issues = await adapter.GetIssuesInMileStone(repo, milestone);
        }
        catch (GitHubAdapter.ExecutionAbortedException e)
        {
            return e.Reason;
        }

        var groupEngine = new IssueGroupEngine();
        var grouped = groupEngine.GroupIssues(issues.Select(x => new IssueWrapper(x))).ToList();
        var toRemove = new List<IssueGroupEngine.IIssueWrapper>();

        if (!settings.NonInteractive)
        {
            var prompt = new MultiSelectionPrompt<IssueGroupEngine.IIssueWrapper>
            {
                Converter = IssueExtensions.ToMarkup,
                PageSize = 25,
                Title = $"Select issues to remove from milestone {milestone.Title}",
                Mode = SelectionMode.Independent,
            };
            foreach (var g in grouped.OrderBy(x => x.MainIssue.Number))
            {
                prompt.AddChoices(g.MainIssue, n =>
                {
                    foreach (var issue in g.SubIssues)
                    {
                        n.AddChild(issue);
                        prompt.Select(issue);
                    }
                });
            }

            toRemove = AnsiConsole.Prompt(prompt);
        }
        else
        {
            var tree = new Tree($"Milestone: (#{milestone.Number}) {milestone.Title}");
            foreach (var g in grouped.OrderBy(x => x.MainIssue.Number))
            {
                toRemove.AddRange(g.SubIssues);

                var node = tree.AddNode(g.MainIssue.ToMarkup());
                // AnsiConsole.MarkupLine($"[gray] > {g.Key}[/]");
                foreach (var issue in g.SubIssues)
                {
                    node.AddNode(issue.ToMarkup());
                }
            }

            AnsiConsole.Write(tree);
        }

        if (!toRemove.Any())
        {
            return 0;
        }

        AnsiConsole.MarkupLine($"[orange3]Removing the milestone from issues: {toRemove.ToShortMarkup()}[/]");
        if (settings.WhatIf)
        {
            return 0;
        }

        async Task DoRemove(Action? callback = null)
        {
            foreach (var issue in toRemove)
            {
                var group = grouped.First(g => 
                    g.MainIssue.BackingIssue.Id == issue.BackingIssue.Id 
                    || g.SubIssues.Any(i => i.BackingIssue.Id == issue.BackingIssue.Id));
                var comment = issue == group.MainIssue ? settings.TopIssueComment : settings.SubIssueComment;
                if (!string.IsNullOrEmpty(comment))
                {
                    comment = string.Format(comment, group.MainIssue.BackingIssue.HtmlUrl);
                }

                await adapter.RemoveMilestone(repo, issue.BackingIssue, comment);
                callback?.Invoke();
            }
        }
        
        if (settings.NonInteractive)
        {
            await DoRemove();
        }
        else
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var inc = 100d / toRemove.Count;
                    var task = ctx.AddTask($"Cleaning {toRemove.Count} issues");
                    await DoRemove(() =>
                    {
                        task.Increment(inc);
                    });

                    var rest = 100 - task.Percentage;
                    task.Increment(rest);
                });
        }

        return 0;
    }
    
    private class IssueWrapper : IssueGroupEngine.IIssueWrapper
    {
        public IssueWrapper(Issue issue)
        {
            BackingIssue = issue;
        }

        public string Title => BackingIssue.Title;
        public int Number => BackingIssue.Number;
        public Issue BackingIssue { get; }
    }
}