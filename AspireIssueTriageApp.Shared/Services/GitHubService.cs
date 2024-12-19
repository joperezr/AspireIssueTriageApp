using AspireIssueTriageApp.Models;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace AspireIssueTriageApp.Services;

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
    /// Gets all open issues for a specified repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of open issues.</returns>
    public async Task<IReadOnlyList<Issue>> GetAllOpenIssuesForRepository(string owner, string repoName)
    {
        var request = new RepositoryIssueRequest
        {
            State = ItemStateFilter.Open
        };
        return await _client.Issue.GetAllForRepository(owner, repoName, request);
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
    /// Removes a label from a specified issue.
    /// </summary>
    /// <param name="issueNumber">The number of the issue to remove the label from.</param>
    /// <param name="label">The label to remove from the issue.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemoveLabelFromIssueAsync(int issueNumber, string label)
    {
        await _client.Issue.Labels.RemoveFromIssue("dotnet", "aspire", issueNumber, label);
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

    /// <summary>
    /// Gets all comments for a specified issue.
    /// </summary>
    /// <param name="issueNumber">The number of the issue to retrieve.</param>
    /// <returns>An enumerable of string comments.</returns>
    public async Task<IEnumerable<(string, string)>> GetIssueCommentsAsync(int issueNumber)
    {
        var comments = await _client.Issue.Comment.GetAllForIssue("dotnet", "aspire", issueNumber);
        return comments.Select(c => new ValueTuple<string, string>(c.User.Login, c.Body));
    }

    /// <summary>
    /// Gets all area labels.
    /// </summary>
    /// <returns>A list of area labels.</returns>
    public IReadOnlyList<AreaLabel> GetAllAreaLabels()
    {
        return new List<AreaLabel>
        {
            new AreaLabel { Name = "area-docs", Description = "Issues related to missing/wrong documentation or guides." },
            new AreaLabel { Name = "area-samples", Description = "Issues related to wrong and/or missing sample." },
            new AreaLabel { Name = "area-dashboard", Description = "Issues related to the Aspire dashboard or something related to the User Interface." },
            new AreaLabel { Name = "area-acquisition", Description = "Issues related to installing .NET Aspire, or getting its templates." },
            new AreaLabel { Name = "area-meta", Description = "Issues related to NuGet packages shipped by the repository and their dependencies. Especially issues that are generalized to all the packages that we ship." },
            new AreaLabel { Name = "area-app-model", Description = "Issues pertaining to the APIs in Aspire.Hosting, e.g. DistributedApplication. These are the extensions that are used to model user's applications which will then be used to orchestrate them. Also issues that are related to the Aspire App Host (also referred to as apphost)" },
            new AreaLabel { Name = "area-app-testing", Description = "Issues pertaining to the APIs in Aspire.Hosting.Testing. Also issues related to running tests for user's .NET Aspire applications." },
            new AreaLabel { Name = "area-deployment", Description = "Issues related to deploying .NET Aspire applications to any target environment." },
            new AreaLabel { Name = "area-engineering-systems", Description = "Issues related to dotnet/aspire repo-specific issues, e.g. CI/CD, AzDO Pipelines, disabled tests, etc." },
            new AreaLabel { Name = "area-integrations", Description = "Issues related to integrating with other systems, e.g. Azure Service Bus, Azure Storage, etc." },
            new AreaLabel { Name = "area-orchestrator", Description = "Issues related to DCP (Developer Control Plane), which is .NET Aspire's orchestrator which launches projects and docker containers." },
            new AreaLabel { Name = "area-service-discovery", Description = "Issues related to our ServiceDiscovery libraries and logic for discovering other services in your application." },
            new AreaLabel { Name = "area-telemetry", Description = "Issues related to Logging, Telemetry, and traces that are sent to the OTel collector and displayed in the .NET Aspire Dashboard." },
            new AreaLabel { Name = "area-templates", Description = "Issues related to the .NET Aspire project templates and their documentation." },
            new AreaLabel { Name = "area-tooling", Description = "Issues related to Visual Studio, Visual Studio Code, or similar tooling that can be used to develop .NET Aspire Applications" }
        };
    }
}
