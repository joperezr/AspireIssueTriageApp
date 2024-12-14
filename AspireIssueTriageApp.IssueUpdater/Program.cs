using AspireIssueTriageApp.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient<IssuesAPIClient>(client =>
{
    client.BaseAddress = new Uri("https+http://issue-api");
});

builder.Services.AddHostedService<IssueUpdaterService>();

builder.Services.AddTransient<GitHubService>();

var host = builder.Build();
host.Run();
