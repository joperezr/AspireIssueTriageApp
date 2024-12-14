var builder = DistributedApplication.CreateBuilder(args);

var issuesAPI = builder.AddProject<Projects.AspireIssueTriageApp_IssueService>("issue-api");

builder.AddProject<Projects.AspireIssueTriageApp_FrontEnd>("frontend")
    .WaitFor(issuesAPI)
    .WithReference(issuesAPI);

builder.AddProject<Projects.AspireIssueTriageApp_IssueProcessor>("issue-processor")
    .WaitFor(issuesAPI)
    .WithReference(issuesAPI);

builder.AddProject<Projects.AspireIssueTriageApp_IssueUpdater>("issue-updater")
    .WaitFor(issuesAPI)
    .WithReference(issuesAPI);

builder.Build().Run();
