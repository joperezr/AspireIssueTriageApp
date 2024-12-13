using AspireIssueTriageApp.FrontEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Octokit;

/// <summary>
/// Provides services for interacting with chat clients and triaging GitHub issues.
/// </summary>
public partial class ChatService
{
    private readonly IChatClient _chatClient;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<ChatService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatService"/> class.
    /// </summary>
    /// <param name="chatClient">The chat client to be used for AI interactions.</param>
    /// <param name="dbContextFactory">The factory to create database contexts.</param>
    /// <param name="logger">The logger to log information.</param>
    public ChatService(IChatClient chatClient, IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<ChatService> logger)
    {
        _chatClient = chatClient;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Triages a GitHub issue using AI and saves the result to the database.
    /// </summary>
    /// <param name="issue">The GitHub issue to be triaged.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the triaged <see cref="GitHubIssue"/>.</returns>
    public async Task<GitHubIssue> TriageIssueAsync(Issue issue)
    {
        Log.TriagingIssue(_logger, issue.Number);

        var prompt = $"You are an AI assistant helping to triage GitHub issues for the dotnet/aspire repository. Given the following issue, return a GitHubIssue model with the fields filled out based on your interpretation.\n\n" +
                     $"Issue Title: {issue.Title}\n" +
                     $"Issue Body: {issue.Body}\n" +
                     $"Issue Labels: {string.Join(", ", issue.Labels.Select(l => l.Name))}\n" +
                     $"Issue Comments: {issue.Comments}\n" +
                     $"Issue URL: {issue.HtmlUrl}\n\n" +
                     "Do not fill in the ID Field. I want you to provide the reasoning in the Reasoning field of why you decided that the issue was a bug, a question, or a feature request.\n" +
                     "In the Summary field, make sure you call out not just the summary of the issue body, but also a quick summary of the conversation in the issue if any." +
                     "Also always leave the 'IsTriaged' field as false, as you will only help provide info for me to triage the issue, but I still have to do it myself.";

        var gitHubIssue = await _chatClient.CompleteAsync<GitHubIssue>(prompt);
        var gitHubIssueResult = gitHubIssue.Result;
        gitHubIssueResult.Id = 0;
        gitHubIssueResult.IsTriaged = false;

        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            dbContext.GitHubIssues.Add(gitHubIssueResult);
            await dbContext.SaveChangesAsync();
        }

        return gitHubIssueResult;
    }

    /// <summary>
    /// Provides logging functionality for the <see cref="ChatService"/> class.
    /// </summary>
    static partial class Log
    {
        /// <summary>
        /// Logs the triaging of an issue.
        /// </summary>
        /// <param name="logger">The logger to log information.</param>
        /// <param name="issueNumber">The number of the issue being triaged.</param>
        [LoggerMessage(0, LogLevel.Information, "Triaging issue {IssueNumber}")]
        public static partial void TriagingIssue(ILogger logger, int issueNumber);
    }
}
