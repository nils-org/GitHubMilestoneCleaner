using System;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using Polly;
using Polly.Retry;
using Spectre.Console;

namespace GitHubMilestoneCleaner.Extension;

public static class GitHubClientExtensions
{
    private const string CancellationTokenSourceKey = "cancellationToken"; 
    private static readonly AsyncRetryPolicy Retry = 
        Policy
            .Handle<ApiException>()
            .WaitAndRetryAsync(
                10,
                (_, exception, _) =>
                {
                    var secondsToWait = exception switch
                    {
                        AbuseException ex1 =>
                            ex1.RetryAfterSeconds.HasValue
                                ? ex1.RetryAfterSeconds.Value * 1.5
                                : 30,
                        RateLimitExceededException ex2 => 
                            (ex2.Reset - DateTimeOffset.Now).TotalSeconds * 1.5,
                        _ => 30,
                    };

                    return TimeSpan.FromSeconds(secondsToWait);
                },
                async (ex, _, _, ctx) =>
                {
                    var cancellationTokenSource = (CancellationTokenSource)ctx[CancellationTokenSourceKey];
                    if (ex is Octokit.NotFoundException)
                    {
                        AnsiConsole.MarkupLine($"[red]{ex.GetType().Name}: {ex.Message}[/]");
                        cancellationTokenSource.Cancel();
                    }
                });
        
    public static async Task<T> WithRetry<T>(
        this GitHubClient client,
        Func<GitHubClient, Task<T>> operation)
    {
        var trappedClient = client;
        var cancellationTokenSource = new CancellationTokenSource();
        var policyContext = new Context("RetryContext") 
        {
            { CancellationTokenSourceKey, cancellationTokenSource }, 
        };
        return await Retry.ExecuteAsync(
            async (ctx, _) => await operation(trappedClient),
            policyContext,
            cancellationTokenSource.Token);
    }
}