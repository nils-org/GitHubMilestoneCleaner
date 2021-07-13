using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubBumpVersionCleaner.Commands
{
    internal sealed class ListCommand : AsyncCommand<ListCommand.Settings>
    {
        public sealed class Settings : CommonCommandSettings
        {
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            return CommonCommandSettings.Validate(context, settings);
        }

        public override async Task<int> ExecuteAsync([NotNull]CommandContext context, [NotNull]Settings settings)
        {

            Milestone milestone;
            IEnumerable<Issue> issues;
            try
            {
                var adapter = new GitHubAdapter(settings.Token);
                var repo = await adapter.GetRepository(settings.Owner, settings.Repository);
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
            if (toRemove.Count > 0)
            {
                AnsiConsole.MarkupLine($"[orange3]The following issues could be removed from the milestone: {toRemove.ToShortMarkup()}[/]");
            }
            
            return 0;
        }

       
    }
}