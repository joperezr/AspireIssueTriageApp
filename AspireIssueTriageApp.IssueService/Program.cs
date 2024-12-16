using AspireIssueTriageApp.DBContext;
using AspireIssueTriageApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), "aspire-issue-triage.db")}"));

builder.Services.AddControllers();

builder.Services.AddAuthorizationBuilder();
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddDbContextFactory<AuthDbContext>(options =>
{
    options.UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), "aspire-issue-triage-auth.db")}");
    options.UseOpenIddict();
});

builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddOpenIddict()
    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
        options.UseEntityFrameworkCore()
                .UseDbContext<AuthDbContext>();

        // Enable Quartz.NET integration.
        options.UseQuartz();
    })
    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the token endpoint.
        options.SetTokenEndpointUris("connect/token");

        // Enable the client credentials flow.
        options.AllowClientCredentialsFlow();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
                .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
                .EnableTokenEndpointPassthrough();
    })
    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddHostedService<AuthDatabaseInitializer>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
