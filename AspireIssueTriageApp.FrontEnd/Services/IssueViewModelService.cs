using AspireIssueTriageApp.FrontEnd.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AspireIssueTriageApp.FrontEnd.Services
{
    /// <summary>
    /// Provides services for interacting with GitHub issues and preparing data for views.
    /// </summary>
    public partial class IssueViewModelService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<IssueViewModelService> _logger;
        private readonly GitHubService _gitHubService;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueViewModelService"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The factory to create database contexts.</param>
        /// <param name="logger">The logger to log information.</param>
        /// <param name="gitHubService">The service to interact with GitHub API.</param>
        public IssueViewModelService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<IssueViewModelService> logger, GitHubService gitHubService)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _gitHubService = gitHubService;
        }

        /// <summary>
        /// Gets all GitHub issues from the database and includes the issue title fetched from GitHub.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="IssueDetails"/>.</returns>
        public async Task<List<IssueDetails>> GetAllIssuesAsync()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var issues = await dbContext.GitHubIssues.AsNoTracking().ToListAsync();
                var issueDetailsList = new List<IssueDetails>();
                foreach (var issue in issues)
                {
                    issueDetailsList.Add(new IssueDetails(await GetIssueTitleAsync(issue.Url), issue));
                }

                return issueDetailsList;
            }
        }

        /// <summary>
        /// Gets the title of a GitHub issue by its URL.
        /// </summary>
        /// <param name="issueUrl">The URL of the issue.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the title of the issue.</returns>
        private async Task<string> GetIssueTitleAsync(string issueUrl)
        {
            Regex regex = IssueNumberExtractor();
            var issue = await _gitHubService.GetIssueAsync(int.Parse(regex.Match(issueUrl).Groups[1].Value));
            return issue.Title;
        }

        /// <summary>
        /// Extracts the issue number from the issue URL using a regular expression.
        /// </summary>
        /// <returns>A regular expression to extract the issue number.</returns>
        [GeneratedRegex(@".*/issues/(\d+)$")]
        private static partial Regex IssueNumberExtractor();
    }

    /// <summary>
    /// Represents the details of a GitHub issue, including the title and database issue information.
    /// </summary>
    /// <param name="Title">The title of the issue.</param>
    /// <param name="DBIssue">The issue information from the database.</param>
    public class IssueDetails(string Title, GitHubIssue DBIssue)
    {
    }
}
