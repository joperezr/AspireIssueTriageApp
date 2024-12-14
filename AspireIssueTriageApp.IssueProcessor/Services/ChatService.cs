using AspireIssueTriageApp.Models;
using Microsoft.Extensions.AI;
using Octokit;

namespace AspireIssueTriageApp.Services;

/// <summary>
/// Provides services for interacting with chat clients and triaging GitHub issues.
/// </summary>
public partial class ChatService(IChatClient chatClient, ILogger<ChatService> logger, IssuesAPIClient issuesAPIClient)
{
    /// <summary>
    /// Triages a GitHub issue using AI and saves the result to the database.
    /// </summary>
    /// <param name="issue">The GitHub issue to be triaged.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the triaged <see cref="GitHubIssue"/>.</returns>
    public async Task<GitHubIssue> TriageIssueAsync(Issue issue)
    {
        Log.TriagingIssue(logger, issue.Number);

        var prompt = $"You are an AI assistant helping to triage GitHub issues for the dotnet/aspire repository. Given the following issue, return a GitHubIssue model with the fields filled out based on your interpretation.\n\n" +
                     $"Issue Title: {issue.Title}\n" +
                     $"Issue Body: {issue.Body}\n" +
                     $"Issue Labels: {string.Join(", ", issue.Labels.Select(l => l.Name))}\n" +
                     $"Issue Comments: {issue.Comments}\n" +
                     $"Issue URL: {issue.HtmlUrl}\n\n" +
                     "Do not fill in the ID Field. I want you to provide the reasoning in the Reasoning field of why you decided that the issue was a bug, a question, or a feature request.\n" +
                     "In the Summary field, make sure you call out not just the summary of the issue body, but also a quick summary of the conversation in the issue if any." +
                     "Also always leave the 'IsTriaged' field as false, as you will only help provide info for me to triage the issue, but I still have to do it myself.";

        var gitHubIssue = await chatClient.CompleteAsync<GitHubIssue>(prompt);
        var gitHubIssueResult = gitHubIssue.Result;
        gitHubIssueResult.Id = 0;
        gitHubIssueResult.IsTriaged = false;

        await issuesAPIClient.CreateIssueAsync(gitHubIssueResult);

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
