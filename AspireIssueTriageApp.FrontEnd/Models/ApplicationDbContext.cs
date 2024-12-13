using AspireIssueTriageApp.FrontEnd.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents the database context for the application, providing access to GitHub issues.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet of GitHub issues.
    /// </summary>
    public DbSet<GitHubIssue> GitHubIssues { get; set; }
}
   