using AspireIssueTriageApp.FrontEnd.Components;
using AspireIssueTriageApp.FrontEnd.Services;
using AspireIssueTriageApp.Services;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddIssuesAPIClient();

builder.Services.AddSingleton(new OpenAIClient(builder.Configuration.GetValue<string>("OPENAI_API_KEY")));

builder.Services.AddChatClient(services => services.GetRequiredService<OpenAIClient>().AsChatClient("gpt-4o"));

builder.Services.AddTransient<IssueViewModelService>();

builder.Services.AddTransient<GitHubService>();

builder.Services.AddSingleton<AreaLabelSuggestionClient>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
