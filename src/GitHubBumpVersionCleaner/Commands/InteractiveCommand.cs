using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubBumpVersionCleaner.Commands
{
    internal sealed class InteractiveCommand : AsyncCommand<InteractiveCommand.Settings>
    {
        public sealed class Settings : CommonCommandSettings
        {
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            return CommonCommandSettings.Validate(context, settings);
        }

        public override async Task<int> ExecuteAsync([NotNull]CommandContext context, [NotNull]Settings commandSettings)
        {
            var adapter = new GitHubAdapter(commandSettings.Token);
            Milestone milestone;
            IEnumerable<Issue> issues;
            Repository repo;
            try
            {
                repo = await adapter.GetRepository(commandSettings.Owner, commandSettings.Repository);
                milestone = await adapter.GetMilestone(repo, commandSettings.Milestone, commandSettings.SearchClosedMilestones);
                issues = await adapter.GetIssuesInMileStone(repo, milestone);
            }
            catch (GitHubAdapter.ExecutionAbortedException e)
            {
                return e.Reason;
            }
            
            var groupEngine = new IssueGroupEngine();
            var grouped = groupEngine.GroupIssues(issues);

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

            var toRemove = AnsiConsole.Prompt(prompt);
            if (toRemove.Count > 0)
            {
                AnsiConsole.MarkupLine($"[orange3]Removing the milestone from issues: {toRemove.ToShortMarkup()}[/]");
                await adapter.RemoveMilestone(repo, toRemove);
            }
            
            return 0;
        }
    }
}