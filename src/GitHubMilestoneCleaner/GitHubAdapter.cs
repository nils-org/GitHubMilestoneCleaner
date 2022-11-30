using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GitHubMilestoneCleaner.Extension;
using Octokit;
using Spectre.Console;

namespace GitHubMilestoneCleaner;

public class GitHubAdapter
{
    private readonly GitHubClient _client;

    public GitHubAdapter(string token)
    {
        _client = new GitHubClient(new ProductHeaderValue(GetAppName()))
        {
            Credentials = new Credentials(token),
        };
    }
        
    public async Task<Repository> GetRepository(
        string owner, 
        string name)
    {
        try
        {
            return await _client.Repository.Get(owner, name);
        }
        catch (Exception e)
        {
            // probably no unauthorized or something like that.
            AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
            throw new ExecutionAbortedException(1);
        }
    }

    public async Task<Milestone> GetMilestone(Repository repo, string name, bool searchClosedMilestones)
    {
        var milestoneRequest = new MilestoneRequest
        {
            State = searchClosedMilestones ? ItemStateFilter.All : ItemStateFilter.Open 
        };
        var milestones = await _client.Issue.Milestone.GetAllForRepository(repo.Id, milestoneRequest);
        var milestone = milestones.FirstOrDefault(x =>
            x.Title.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (milestone == null)
        {
            AnsiConsole.MarkupLine("[red]Could not find milestone.[/]");
            throw new ExecutionAbortedException(2);
        }

        return milestone;
    }
        
    public async Task<IEnumerable<Issue>> GetIssuesInMileStone(Repository repo, Milestone milestone)
    {
        var issueRequest = new RepositoryIssueRequest
        {
            Filter = IssueFilter.All,
            State = ItemStateFilter.All,
            Milestone = milestone.Number.ToString(CultureInfo.InvariantCulture)
        };
        var issues = 
            await _client.WithRetry(c => c.Issue.GetAllForRepository(repo.Id, issueRequest));

        return issues;
    }

    public async Task RemoveMilestone(
        Repository repo,
        Issue issue,
        string? comment)
    {
        if (!string.IsNullOrEmpty(comment))
        {
            await _client.WithRetry(c => 
                c.Issue.Comment.Create(
                    repo.Id,
                    issue.Number,
                    comment));
        }

        var update = issue.ToUpdate();
        update.Milestone = null;
        await _client.WithRetry(c => c.Issue.Update(repo.Id, issue.Number, update));
    }

    public class ExecutionAbortedException : Exception
    {
        public int Reason { get; }

        public ExecutionAbortedException(int reason)
        {
            Reason = reason;
        }
    }

    private string GetAppName()
    {
        var name = GetType().Assembly.GetName();
        return $"{name.Name}-{name.Version}";
    }
}