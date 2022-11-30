using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Octokit;

namespace GitHubMilestoneCleaner.Engines;

public class IssueGroupEngine
{
    private  readonly Regex _versionMatcher =
        new Regex(
            @"(0|[1-9]\d*)(\.(0|[1-9]\d*))+(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?");
        
    public IEnumerable<IssueGroup> GroupIssues(IEnumerable<Issue> issues)
    {

        return issues
            .Select(x => new
            {
                Issue = x,
                VersionAgnosticName = _versionMatcher.Replace(x.Title, string.Empty)
            })
            .GroupBy(x => x.VersionAgnosticName)
            .Select(x =>
            {
                var issuesInGroup = 
                    x.ToList()
                        .OrderByDescending(y => y.Issue.Number)
                        .ToList();
                var main = issuesInGroup.First();
                var subIssues = issuesInGroup.Skip(1).Select(y => y.Issue).ToList();

                return new IssueGroup
                {
                    MatchKey = main.VersionAgnosticName,
                    MainIssue = main.Issue,
                    SubIssues = subIssues,
                };
            });
    }

    public class IssueGroup
    {
        public string MatchKey { get; set; }
        public Issue MainIssue { get; set; }
        public IEnumerable<Issue> SubIssues { get; set; }
    }
}