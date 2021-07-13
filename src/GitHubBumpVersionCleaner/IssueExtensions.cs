using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace GitHubBumpVersionCleaner
{
    internal static class IssueExtensions
    {
        internal static string GetWebUrl(this Issue issue)
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
        
        internal static string ToShortMarkup(this Issue issue)
        {
            return $"[link={issue.GetWebUrl()}]#{issue.Number}[/]";
        }

        internal static string ToShortMarkup(this IEnumerable<Issue> issues)
        {
            return string.Join(
                ", ", 
                issues
                    .OrderBy(x => x.Number)
                    .Select(x => x.ToShortMarkup()));
        }

    }
}