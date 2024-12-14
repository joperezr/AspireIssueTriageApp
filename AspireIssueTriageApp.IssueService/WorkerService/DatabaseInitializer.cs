using Microsoft.EntityFrameworkCore;

namespace AspireIssueTriageApp.Services;

internal class DatabaseInitializer(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}