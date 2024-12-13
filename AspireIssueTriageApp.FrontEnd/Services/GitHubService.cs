using Octokit;

/// <summary>
/// Service for interacting with the GitHub API to manage issues in the dotnet/aspire repository.
/// </summary>
public class GitHubService
{
    private readonly GitHubClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to retrieve the GitHub access token.</param>
    public GitHubService(IConfiguration configuration)
    {
        var token = configuration["GH_TOKEN"];
        _client = new GitHubClient(new ProductHeaderValue("AspireIssueTriageApp"))
        {
            Credentials = new Credentials(token)
        };
    }

    /// <summary>
    /// Gets the list of untriaged issues from the dotnet/aspire repository.
    /// </summary>
    /// <returns>A list of untriaged issues.</returns>
    public async Task<IReadOnlyList<Issue>> GetUntriagedIssuesAsync()
    {
        var request = new RepositoryIssueRequest
        {
            State = ItemStateFilter.Open
        };
        request.Labels.Add("untriaged");
        return await _client.Issue.GetAllForRepository("dotnet", "aspire", request);
    }

    /// <summary>
    /// Adds a label to a specified issue.
    /// </summary>
    /// <param name="issueNumber">The number of the issue to label.</param>
    /// <param name="label">The label to add to the issue.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddLabelToIssueAsync(int issueNumber, string label)
    {
        await _client.Issue.Labels.AddToIssue("dotnet", "aspire", issueNumber, new[] { label });
    }

    /// <summary>
    /// Adds a comment to a specified issue.
    /// </summary>
    /// <param name="issueNumber">The number of the issue to comment on.</param>
    /// <param name="comment">The comment to add to the issue.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddCommentToIssueAsync(int issueNumber, string comment)
    {
        await _client.Issue.Comment.Create("dotnet", "aspire", issueNumber, comment);
    }

    /// <summary>
    /// Assigns a milestone to a specified issue.
    /// </summary>
    /// <param name="issueNumber">The number of the issue to assign the milestone to.</param>
    /// <param name="milestoneNumber">The number of the milestone to assign to the issue.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AssignMilestoneToIssueAsync(int issueNumber, int milestoneNumber)
    {
        var issueUpdate = new IssueUpdate { Milestone = milestoneNumber };
        await _client.Issue.Update("dotnet", "aspire", issueNumber, issueUpdate);
    }

    /// <summary>
    /// Closes a specified issue.
    /// </summary>
    /// <param name="issueNumber">The number of the issue to close.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CloseIssueAsync(int issueNumber)
    {
        var issueUpdate = new IssueUpdate { State = ItemState.Closed };
        await _client.Issue.Update("dotnet", "aspire", issueNumber, issueUpdate);
    }

    /// <summary>
    /// Gets a specified issue from the dotnet/aspire repository.
    /// </summary>
    /// <param name="issueNumber">The number of the issue to retrieve.</param>
    /// <returns>The issue object.</returns>
    public async Task<Issue> GetIssueAsync(int issueNumber)
    {
        return await _client.Issue.Get("dotnet", "aspire", issueNumber);
    }
}
