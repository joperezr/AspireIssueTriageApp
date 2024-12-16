var builder = DistributedApplication.CreateBuilder(args);

var issuesAPI = builder.AddProject<Projects.AspireIssueTriageApp_IssueService>("issue-api");

builder.AddProject<Projects.AspireIssueTriageApp_FrontEnd>("frontend")
    .WaitFor(issuesAPI)
    .WithReference(issuesAPI)
    .WithEnvironment(e =>
    {
        e.EnvironmentVariables.Add("issues-api-endpoint", issuesAPI.GetEndpoint("https"));
    });;

builder.AddProject<Projects.AspireIssueTriageApp_IssueProcessor>("issue-processor")
    .WaitFor(issuesAPI)
    .WithReference(issuesAPI)
    .WithEnvironment(e =>
    {
        e.EnvironmentVariables.Add("issues-api-endpoint", issuesAPI.GetEndpoint("https"));
    });;

builder.AddProject<Projects.AspireIssueTriageApp_IssueUpdater>("issue-updater")
    .WaitFor(issuesAPI)
    .WithReference(issuesAPI)
    .WithEnvironment(e =>
    {
        e.EnvironmentVariables.Add("issues-api-endpoint", issuesAPI.GetEndpoint("https"));
    });

builder.Build().Run();
