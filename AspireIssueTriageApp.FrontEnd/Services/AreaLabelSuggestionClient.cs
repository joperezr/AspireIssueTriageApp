using AspireIssueTriageApp.Models;
using Microsoft.Extensions.AI;

namespace AspireIssueTriageApp.Services;

public partial class AreaLabelSuggestionClient(IChatClient chatClient, GitHubService gitHubService, ILogger<AreaLabelSuggestion> logger)
{
    const string Prompt = $"""
            You are an AI assistant that suggest area labels for GitHub issues in the dotnet/aspire repository (.NET Aspire). Your task is to suggest a collection of area labels for a given GitHub issue based on its title, description, and comments.
            The area label should be one of the following labels(expressed as json with label name and description):
            {AreaLabels}

                You will receive a request with the following fields:
                - Title: The title of the GitHub issue.
                - Description: The description of the GitHub issue.
                - Comments: A list of comments on the GitHub issue.

                Your response should be a JSON array with objects with the following fields:
                - AreaLabel: The suggested area label for the GitHub issue.
                - Reasoning: A brief explanation of why you chose this area label.
                - Confidence: A confidence score from 0 to 1 indicating how certain you are about your suggestion (0 being not confident at all, and 1 being fully confident).

                Please provide your response in JSON format.
            """;

    const string AreaLabels = """
            [
                {
                    "name": "area-docs",
                    "description": "Issues related to missing/wrong documentation or guides."
                },
                {
                    "name": "area-samples",
                    "description": "Issues related to wrong and/or missing sample."
                },
                {
                    "name": "area-dashboard",
                    "description": "Issues related to the Aspire dashboard or something related to the User Interface."
                },
                {
                    "name": "area-acquisition",
                    "description": "Issues related to installing .NET Aspire, or getting its templates."
                },
                {
                    "name": "area-meta",
                    "description": "Issues related to NuGet packages shipped by the repository and their dependencies. Especially issues that are generalized to all the packages that we ship."
                },
                {
                    "name": "area-app-model",
                    "description": "Issues pertaining to the APIs in Aspire.Hosting, e.g. DistributedApplication. These are the extensions that are used to model user's applications which will then be used to orchestrate them. Also issues that are related to the Aspire App Host (also referred to as apphost)"
                },
                {
                    "name": "area-app-testing",
                    "description": "Issues pertaining to the APIs in Aspire.Hosting.Testing. Also issues related to running tests for user's .NET Aspire applications."
                },
                {
                    "name": "area-deployment",
                    "description": "Issues related to deploying .NET Aspire applications to any target environment."
                },
                {
                    "name": "area-engineering-systems",
                    "description": "Issues related to dotnet/aspire repo-specific issues, e.g. CI/CD, AzDO Pipelines, disabled tests, etc."
                },
                {
                    "name": "area-integrations",
                    "description": "Issues related to integrating with other systems, e.g. Azure Service Bus, Azure Storage, etc."
                },
                {
                    "name": "area-orchestrator",
                    "description": "Issues related to DCP (Developer Control Plane), which is .NET Aspire's orchestrator which launches projects and docker containers."
                },
                {
                    "name": "area-service-discovery",
                    "description": "Issues related to our ServiceDiscovery libraries and logic for discovering other services in your application."
                },
                {
                    "name": "area-telemetry",
                    "description": "Issues related to Logging, Telemetry, and traces that are sent to the OTel collector and displayed in the .NET Aspire Dashboard."
                },
                {
                    "name": "area-templates",
                    "description": "Issues related to the .NET Aspire project templates and their documentation."
                },
                {
                    "name": "area-tooling",
                    "description": "Issues related to Visual Studio, Visual Studio Code, or similar tooling that can be used to develop .NET Aspire Applications"
                }
            ]
            """;

    public async Task<IEnumerable<AreaLabelSuggestion>> GetAreaLabelSuggestionAsync(GitHubIssue issue)
    {
        var githubIssue = await gitHubService.GetIssueAsync(issue.Number);

        var comments = (await gitHubService.GetIssueCommentsAsync(issue.Number)).Select(c => c.Item2);

        var issueRequest = new GitHubIssueAreaLabelRequest(githubIssue.Title, githubIssue.Body, comments);

        var prompt = new ChatMessage(ChatRole.System, Prompt);

        var request = new ChatMessage(ChatRole.User, $"""
            Help me suggest the best area labels for the following GitHub issue (serialized as json):
            {System.Text.Json.JsonSerializer.Serialize(issueRequest)}
        """);

        try
        {
            // var response = await chatClient.CompleteAsync<AreaLabelSuggestion>(new []{prompt, request}, cancellationToken: CancellationToken.None, useNativeJsonSchema: true);
            // if (response.Result is not null){
            //     return [ response.Result];
            // } else {
            //     return [];
            // }
            var response = await chatClient.CompleteAsync<IEnumerable<AreaLabelSuggestion>>(new []{prompt, request}, cancellationToken: CancellationToken.None, useNativeJsonSchema: true);
            if (response.Result is not null)
            {
                return response.Result;
            }
            else
            {
                return [];
            }
        } catch (Exception ex)
        {
            LogErrorGettingAreaLabelSuggestion(ex);
            return [];
        }
    }

    [LoggerMessage(0, LogLevel.Error, "Error getting area label suggestion")]
    partial void LogErrorGettingAreaLabelSuggestion(Exception ex);
}

public record AreaLabelSuggestion(string AreaLabel, string Reasoning, double Confidence);

public record GitHubIssueAreaLabelRequest(string Title, string Description, IEnumerable<string> Comments);
