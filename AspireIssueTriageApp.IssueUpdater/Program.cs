using AspireIssueTriageApp.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddIssuesAPIClient();

builder.Services.AddHostedService<IssueUpdaterService>();

builder.Services.AddTransient<GitHubService>();

var host = builder.Build();
host.Run();
