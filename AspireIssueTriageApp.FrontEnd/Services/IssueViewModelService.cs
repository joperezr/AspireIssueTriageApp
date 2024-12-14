using AspireIssueTriageApp.Models;
using AspireIssueTriageApp.Services;

namespace AspireIssueTriageApp.FrontEnd.Services
{
    /// <summary>
    /// Provides services for interacting with GitHub issues and preparing data for views.
    /// </summary>
    public partial class IssueViewModelService(IssuesAPIClient issuesAPIClient)
    {
        /// <summary>
        /// Gets all GitHub issues from the database and includes the issue title fetched from GitHub.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="IssueDetails"/>.</returns>
        public async Task<List<IssueDetails>> GetAllIssuesAsync()
        {
            var issues = await issuesAPIClient.GetIssuesAsync();
            var issueDetailsList = new List<IssueDetails>();
            foreach (var issue in issues)
            {
                issueDetailsList.Add(new IssueDetails(issue.Title, issue));
            }

            return issueDetailsList;
        }
    }

    /// <summary>
    /// Represents the details of a GitHub issue, including the title and database issue information.
    /// </summary>
    /// <param name="Title">The title of the issue.</param>
    /// <param name="DBIssue">The issue information from the database.</param>
    public record IssueDetails(string Title, GitHubIssue DBIssue) { }
}
