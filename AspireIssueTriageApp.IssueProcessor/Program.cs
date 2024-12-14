using AspireIssueTriageApp.Services;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddSingleton(new OpenAIClient(builder.Configuration.GetValue<string>("OPENAI_API_KEY")));

builder.Services.AddChatClient(services => services.GetRequiredService<OpenAIClient>().AsChatClient("gpt-4o"));

builder.Services.AddHttpClient<IssuesAPIClient>(client =>
{
    client.BaseAddress = new Uri("https+http://issue-api");
});

builder.Services.AddHostedService<IssueProcessingService>();

// Register the GitHub service as Transient
builder.Services.AddTransient<GitHubService>();

builder.Services.AddTransient<ChatService>();

var host = builder.Build();
host.Run();
