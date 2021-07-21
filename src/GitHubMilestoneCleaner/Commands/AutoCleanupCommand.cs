using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubMilestoneCleaner.Commands
{
    internal sealed class AutoCleanupCommand : AsyncCommand<AutoCleanupCommand.Settings>
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

            var adapter = new GitHubAdapter(settings.Token);
            Repository repo;
            IEnumerable<Issue> issues;
            try
            {
                repo = await adapter.GetRepository(settings.Owner, settings.Repository);
                var milestone = await adapter.GetMilestone(repo, settings.Milestone, settings.SearchClosedMilestones);
                issues = await adapter.GetIssuesInMileStone(repo, milestone);
            }
            catch (GitHubAdapter.ExecutionAbortedException e)
            {
                return e.Reason;
            }
            
            var groupEngine = new IssueGroupEngine();
            var grouped = groupEngine.GroupIssues(issues);
            var toRemove = grouped.SelectMany(x => x.SubIssues);
            if (toRemove.Any())
            { ;
                AnsiConsole.MarkupLine($"[orange3]Removing the milestone from issues: {toRemove.ToShortMarkup()}[/]");
                await adapter.RemoveMilestone(repo, toRemove);
            }
            
            return 0;
        }

       
    }
}