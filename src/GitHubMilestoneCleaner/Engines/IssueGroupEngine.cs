using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Octokit;

namespace GitHubMilestoneCleaner.Engines;

public class IssueGroupEngine
{
    private  readonly Regex _versionMatcher =
        new(
            @"\s*(from|to) v?(0|[1-9]\d*)(\.(0|[1-9]\d*))*(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?");
    private  readonly Regex _digestMatcher = new(@"\s*digest to [0-9a-fA-F]+$");

        
    public IEnumerable<IssueGroup> GroupIssues(IEnumerable<IIssueWrapper> issues)
    {
        var matchers = new[]
        {
            _versionMatcher,
            _digestMatcher
        };
        
        return issues
            .Select(x => new
            {
                Issue = x,
                VersionAgnosticName = (matchers
                    .Select(m => new
                    {
                        Matcher = m,
                        Matches = m.Matches(x.Title)
                    })
                    .Where(y => y.Matches.Count > 0)
                    .MaxBy(m => m.Matches[0].Length)? // Really, the longest match is always the best??
                    .Matcher.Replace(x.Title, string.Empty) ?? x.Title)
                    .Trim(),
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

    public record IssueGroup
    {
        public string MatchKey { get; init; } = default!;
        public IIssueWrapper MainIssue { get; init; } = default!;
        public IEnumerable<IIssueWrapper> SubIssues { get; init; } = default!;
    }

    public interface IIssueWrapper
    {
        string Title { get; }
        int Number { get; }
        Issue BackingIssue { get; }
    }
}
