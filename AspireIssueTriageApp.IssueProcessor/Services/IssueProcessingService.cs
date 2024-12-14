namespace AspireIssueTriageApp.Services;

public partial class IssueProcessingService(ILogger<IssueProcessingService> logger, ChatService chatService, GitHubService gitHubService, IssuesAPIClient issuesAPIClient) : BackgroundService
{
    /// <summary>
    /// This method is called when the <see cref="IHostedService"/> starts. It creates the database if it doesn't exist and starts processing issues.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> that indicates when the service should stop.</param>
    /// <returns>A <see cref="Task"/> that represents the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessIssuesAsync();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    /// <summary>
    /// Processes the GitHub issues. This method should contain the actual issue processing logic.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private async Task ProcessIssuesAsync()
    {
        Log.ProcessingIssues(logger);
        var issues = await gitHubService.GetUntriagedIssuesAsync();
        int totalCount = issues.Count;
        int i = 0;

        HashSet<string> existingIssueUrls = (await issuesAPIClient.GetIssuesAsync()).Select(i => i.Url).ToHashSet();

        foreach (var issue in issues)
        {
            Log.ProcessingIssue(logger, i++, totalCount);
            if (!existingIssueUrls.Contains(issue.HtmlUrl))
            {
                try
                {
                    _ = await chatService.TriageIssueAsync(issue);
                }
                catch (Exception ex)
                {
                    Log.TriageIssueFailed(logger, issue.Number, ex);
                }
            }
            else
            {
                Log.IssueAlreadyProcessed(logger, issue.Number);
            }
        }
        Log.FinishProcessingIssues(logger);
    }

    /// <summary>
    /// Static class for logging messages related to issue processing.
    /// </summary>
    static partial class Log
    {
        /// <summary>
        /// Logs an information message indicating that issues are being processed.
        /// </summary>
        /// <param name="logger">The logger to log the message.</param>
        [LoggerMessage(0, LogLevel.Information, "Beginning processing issues...")]
        public static partial void ProcessingIssues(ILogger logger);

        /// <summary>
        /// Logs an information message indicating that issues are being processed.
        /// </summary>
        /// <param name="logger">The logger to log the message.</param>
        [LoggerMessage(4, LogLevel.Information, "Finished processing issues...")]
        public static partial void FinishProcessingIssues(ILogger logger);

        /// <summary>
        /// Logs an information message indicating the progress of processing an issue.
        /// </summary>
        /// <param name="logger">The logger to log the message.</param>
        /// <param name="i">The current issue index.</param>
        /// <param name="totalCount">The total number of issues.</param>
        [LoggerMessage(1, LogLevel.Information, "Processing issue {i}/{totalCount}")]
        public static partial void ProcessingIssue(ILogger logger, int i, int totalCount);

        /// <summary>
        /// Logs an information message indicating that an issue has already been processed.
        /// </summary>
        /// <param name="logger">The logger to log the message.</param>
        /// <param name="issueNumber">The number of the issue that has already been processed.</param>
        [LoggerMessage(2, LogLevel.Information, "Issue {issueNumber} has already been processed.")]
        public static partial void IssueAlreadyProcessed(ILogger logger, int issueNumber);

        /// <summary>
        /// Logs a warning message indicating that triaging an issue failed.
        /// </summary>
        /// <param name="logger">The logger to log the message.</param>
        /// <param name="issueNumber">The number of the issue that failed to be triaged.</param>
        [LoggerMessage(3, LogLevel.Warning, "Failed to triage issue {issueNumber}")]
        public static partial void TriageIssueFailed(ILogger logger, int issueNumber, Exception exception);
    }
}
