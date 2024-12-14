using AspireIssueTriageApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspireIssueTriageApp.IssueService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController(IDbContextFactory<ApplicationDbContext> contextFactory) : ControllerBase
    {
        // GET: api/Issues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GitHubIssue>>> GetIssues()
        {
            using var context = contextFactory.CreateDbContext();
            return await context.GitHubIssues.ToListAsync();
        }

        // GET: api/Issues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GitHubIssue>> GetIssue(int id)
        {
            using var context = contextFactory.CreateDbContext();
            var issue = await context.GitHubIssues.FindAsync(id);

            if (issue == null)
            {
                return NotFound();
            }

            return issue;
        }

        // GET: api/Issues/by-url
        [HttpGet("by-url")]
        public async Task<ActionResult<GitHubIssue>> GetIssueByUrl([FromQuery] string url)
        {
            using var context = contextFactory.CreateDbContext();
            var issue = await context.GitHubIssues.FirstOrDefaultAsync(i => i.Url == url);

            if (issue == null)
            {
                return NotFound();
            }

            return issue;
        }

        // POST: api/Issues
        [HttpPost]
        public async Task<ActionResult<GitHubIssue>> PostIssue(GitHubIssue issue)
        {
            using var context = contextFactory.CreateDbContext();
            context.GitHubIssues.Add(issue);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIssue), new { id = issue.Id }, issue);
        }

        // PUT: api/Issues/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssue(int id, GitHubIssue issue)
        {
            if (id != issue.Id)
            {
                return BadRequest();
            }

            using var context = contextFactory.CreateDbContext();
            context.Entry(issue).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IssueExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Issues/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id)
        {
            using var context = contextFactory.CreateDbContext();
            var issue = await context.GitHubIssues.FindAsync(id);
            if (issue == null)
            {
                return NotFound();
            }

            context.GitHubIssues.Remove(issue);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool IssueExists(int id)
        {
            using var context = contextFactory.CreateDbContext();
            return context.GitHubIssues.Any(e => e.Id == id);
        }
    }
}
