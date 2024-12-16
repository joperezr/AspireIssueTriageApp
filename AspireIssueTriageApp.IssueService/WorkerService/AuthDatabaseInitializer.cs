using System;
using AspireIssueTriageApp.DBContext;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AspireIssueTriageApp.Services;

internal class AuthDatabaseInitializer(IServiceProvider _serviceProvider, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AuthDbContext>>();
        using var context = contextFactory.CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync(configuration.GetValue<string>("OpenIddict:ClientID")!) == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = configuration.GetValue<string>("OpenIddict:ClientID")!,
                ClientSecret = configuration.GetValue<string>("OpenIddict:ClientSecret")!,
                DisplayName = "frontend",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials
                }
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
