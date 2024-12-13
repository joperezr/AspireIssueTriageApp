using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service that processes GitHub issues at regular intervals.
/// </summary>
public partial class IssueProcessingService : BackgroundService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<IssueProcessingService> _logger;
    private readonly ChatService _chatService;
    private readonly GitHubService _gitHubService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueProcessingService"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The factory to create instances of <see cref="ApplicationDbContext"/>.</param>
    /// <param name="logger">The logger to log information.</param>
    /// <param name="chatService">The chat service to process issues.</param>
    /// <param name="gitHubService">The GitHub service to interact with GitHub issues.</param>
    public IssueProcessingService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<IssueProcessingService> logger, ChatService chatService, GitHubService gitHubService)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _chatService = chatService;
        _gitHubService = gitHubService;
    }

    /// <summary>
    /// This method is called when the <see cref="IHostedService"/> starts. It creates the database if it doesn't exist and starts processing issues.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> that indicates when the service should stop.</param>
    /// <returns>A <see cref="Task"/> that represents the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureDatabaseCreatedAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessIssuesAsync();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    /// <summary>
    /// Ensures that the database is created.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private async Task EnsureDatabaseCreatedAsync()
    {
        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }

    /// <summary>
    /// Processes the GitHub issues. This method should contain the actual issue processing logic.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private async Task ProcessIssuesAsync()
    {
        Log.ProcessingIssues(_logger);
        var issues = await _gitHubService.GetUntriagedIssuesAsync();
        int totalCount = issues.Count;
        int i = 0;

        HashSet<string> existingIssueUrls = await GetExistingIssueUrlsAsync();

        foreach (var issue in issues)
        {
            Log.ProcessingIssue(_logger, i++, totalCount);
            if (!existingIssueUrls.Contains(issue.HtmlUrl))
            {
                try
                {
                    _ = await _chatService.TriageIssueAsync(issue);
                }
                catch (Exception ex)
                {
                    Log.TriageIssueFailed(_logger, issue.Number, ex);
                }
            }
            else
            {
                Log.IssueAlreadyProcessed(_logger, issue.Number);
            }
        }
        Log.FinishProcessingIssues(_logger);
    }

    private async Task<HashSet<string>> GetExistingIssueUrlsAsync()
    {
        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            return new HashSet<string>(await dbContext.GitHubIssues.Select(issue => issue.Url).ToListAsync());
        }
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
        [LoggerMessage(0, LogLevel.Information, "Finished processing issues...")]
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
