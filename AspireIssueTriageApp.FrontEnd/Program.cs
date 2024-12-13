using AspireIssueTriageApp.FrontEnd.Components;
using AspireIssueTriageApp.FrontEnd.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext with SQLite
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), "aspire-issue-triage.db")}"));

builder.Services.AddSingleton(new OpenAIClient(builder.Configuration.GetValue<string>("OPENAI_API_KEY")));

builder.Services.AddChatClient(services => services.GetRequiredService<OpenAIClient>().AsChatClient("gpt-4o"));

// Register the background service
builder.Services.AddHostedService<IssueProcessingService>();

// Register the GitHub service as Transient
builder.Services.AddTransient<GitHubService>();

builder.Services.AddTransient<ChatService>();

builder.Services.AddTransient<IssueViewModelService>();

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
