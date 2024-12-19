using AspireIssueTriageApp.Models;
using Octokit;

namespace AspireIssueTriageApp.Services;

public partial class IssueUpdaterService(ILogger<IssueUpdaterService> logger, IssuesAPIClient issuesAPIClient, GitHubService gitHubClient) : BackgroundService
{
    /// <summary>
    /// Executes the background service to update issues periodically.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogWorkerRunning(logger, DateTimeOffset.Now);
            await UpdateIssuesAsync();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    /// <summary>
    /// Updates the issues by fetching data from GitHub and internal issue tracker.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateIssuesAsync()
    {
        var allIssues = await issuesAPIClient.GetIssuesAsync(pageSize: 5_000);
        var allGitHubIssues = await gitHubClient.GetAllOpenIssuesForRepository("dotnet", "aspire");

        var removeClosedIssuesTask = RemoveClosedAndTriagedIssuesAsync(allIssues, allGitHubIssues);
        var updateIssueDetailsTask = UpdateIssueDetailsAsync(allIssues, allGitHubIssues);

        await Task.WhenAll(removeClosedIssuesTask, updateIssueDetailsTask);
    }

    /// <summary>
    /// Updates the details of the issues.
    /// </summary>
    /// <param name="allIssues">All issues from the internal issue tracker.</param>
    /// <param name="allGitHubIssues">All open issues from GitHub.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateIssueDetailsAsync(IEnumerable<GitHubIssue> allIssues, IReadOnlyList<Issue> allGitHubIssues)
    {
        foreach (var issue in allIssues)
        {
            var openIssue = allGitHubIssues.FirstOrDefault(x => x.HtmlUrl == issue.Url);
            if (openIssue != null)
            {
                if (issue != openIssue)
                {
                    issue.Title = openIssue.Title;
                    issue.Labels = openIssue.Labels.Select(x => x.Name).ToList();
                    issue.Milestone = openIssue.Milestone?.Title;
                    issue.Upvotes = openIssue.Reactions.Plus1;
                    issue.Number = openIssue.Number;

                    LogUpdatingIssue(logger, issue.Url);
                    try
                    {
                        await issuesAPIClient.UpdateIssueAsync(issue.Id, issue);
                    }
                    catch (Exception ex)
                    {
                        LogErrorUpdatingIssue(logger, issue.Url, ex);
                    }
                }
                else
                {
                    LogIssueIsUpToDate(logger, issue.Url);
                }
            }
        }
    }

    private async Task RemoveClosedAndTriagedIssuesAsync(IEnumerable<GitHubIssue> allIssues, IReadOnlyList<Issue> allGitHubIssues)
    {
        foreach (var issue in allIssues)
        {
            if (!allGitHubIssues.Any(x => x.HtmlUrl == issue.Url))
            {
                LogRemoveClosedIssue(logger, issue.Url);
                await issuesAPIClient.DeleteIssueAsync(issue.Id);
            }
            else
            {
                var openIssue = allGitHubIssues.First(x => x.HtmlUrl == issue.Url);
                if (!openIssue.Labels.Any(x => x.Name == "untriaged"))
                {
                    LogRemovedTriagedIssue(logger, issue.Url);
                    await issuesAPIClient.DeleteIssueAsync(issue.Id);
                }
                else
                {
                    LogIssueNotClosedOrTriaged(logger, issue.Url);
                }
            }
        }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Worker running at: {time}")]
    static partial void LogWorkerRunning(ILogger logger, DateTimeOffset time);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Updating issue {IssueUrl} with new details")]
    static partial void LogUpdatingIssue(ILogger logger, string IssueUrl);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Issue {IssueUrl} is up to date.")]
    static partial void LogIssueIsUpToDate(ILogger logger, string IssueUrl);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Removing issue {IssueUrl} as it is closed.")]
    static partial void LogRemoveClosedIssue(ILogger logger, string IssueUrl);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Removing issue {IssueUrl} as it is triaged.")]
    static partial void LogRemovedTriagedIssue(ILogger logger, string IssueUrl);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Issue {IssueUrl} is not closed or triaged.")]
    static partial void LogIssueNotClosedOrTriaged(ILogger logger, string IssueUrl);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Error updating issue {IssueUrl}")]
    static partial void LogErrorUpdatingIssue(ILogger logger, string IssueUrl, Exception exception);
}
