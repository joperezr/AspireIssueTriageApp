using Microsoft.EntityFrameworkCore;

namespace AspireIssueTriageApp.DBContext;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder) { }
}
