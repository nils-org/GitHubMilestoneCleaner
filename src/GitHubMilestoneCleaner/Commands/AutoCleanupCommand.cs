using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GitHubMilestoneCleaner.Engines;
using Octokit;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubMilestoneCleaner.Commands
{
    internal sealed class CleanVersionBumpsCommand : AsyncCommand<CleanVersionBumpsCommand.Settings>
    {
        public sealed class Settings : CommonCommandSettings
        {
            [Description("Interactively select which issues to remove from the milestone.")]
            [CommandOption("-i|--interactive")]
            public bool Interactive { get; set; }

            [Description("Do not actually remove any items, only show what would be removed.")]
            [CommandOption("-w|--whatIf")]
            public bool WhatIf { get; set; }
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            return CommonCommandSettings.Validate(context, settings);
        }

        public override async Task<int> ExecuteAsync([NotNull]CommandContext context, [NotNull]Settings settings)
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
            var grouped = groupEngine.GroupIssues(issues);
            var toRemove = new List<Issue>();

            if (settings.Interactive)
            {
                var prompt = new MultiSelectionPrompt<Issue>
                {
                    Converter = IssueExtensions.ToMarkup,
                    PageSize = 25,
                    Title = $"Select issues to remove from milestone {milestone.Title}",
                    Mode = SelectionMode.Independent
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

                AnsiConsole.Render(tree);
            }
            
            if (toRemove.Any())
            {
                AnsiConsole.MarkupLine($"[orange3]Removing the milestone from issues: {toRemove.ToShortMarkup()}[/]");
                if (!settings.WhatIf)
                {
                    await adapter.RemoveMilestone(repo, toRemove);
                }
            }
            
            return 0;
        }

       
    }
}