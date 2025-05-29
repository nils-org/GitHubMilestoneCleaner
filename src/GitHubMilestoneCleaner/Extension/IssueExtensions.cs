using System.Collections.Generic;
using System.Linq;
using GitHubMilestoneCleaner.Engines;
using Octokit;

namespace GitHubMilestoneCleaner.Extension;

internal static class IssueExtensions
{
    private static string GetWebUrl(this Issue issue)
    {
        // issue.url points to json data
        var url = issue.Url
            .Replace("api.github.com", "github.com")
            .Replace("/repos/", "/");
        return url;
    }
   
    internal static string ToMarkup(this Issue issue)
    {
        return $"[link={issue.GetWebUrl()}][green](#{issue.Number})[/] [yellow]{issue.Title}[/][/]";
    }
    
    internal static string ToMarkup(this IssueGroupEngine.IIssueWrapper issue)
    {
        return ToMarkup(issue.BackingIssue);
    }

    internal static string ToShortMarkup(this Issue issue)
    {
        return $"[link={issue.GetWebUrl()}]#{issue.Number}[/]";
    }
    
    internal static string ToShortMarkup(this IssueGroupEngine.IIssueWrapper issue)
    {
        return ToShortMarkup(issue.BackingIssue);
    }

    internal static string ToShortMarkup(this IEnumerable<Issue> issues)
    {
        return string.Join(
            ", ", 
            issues
                .OrderBy(x => x.Number)
                .Select(x => x.ToShortMarkup()));
    }
    
    internal static string ToShortMarkup(this IEnumerable<IssueGroupEngine.IIssueWrapper> issues)
    {
        return issues.Select(x => x.BackingIssue).ToShortMarkup();
    }
}