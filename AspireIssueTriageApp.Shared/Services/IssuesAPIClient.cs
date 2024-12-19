using System.Net.Http.Json;
using AspireIssueTriageApp.Models;

namespace AspireIssueTriageApp.Services
{
    public class IssuesAPIClient(HttpClient httpClient)
    {
        public async Task<IEnumerable<GitHubIssue>> GetIssuesAsync(int page = 1, int pageSize = 10)
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<GitHubIssue>>($"/api/Issues?page={page}&pageSize={pageSize}") ?? Enumerable.Empty<GitHubIssue>();
        }

        public async Task<GitHubIssue?> GetIssueByIdAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<GitHubIssue>($"/api/Issues/{id}");
        }

        public async Task<GitHubIssue?> GetIssueByUrlAsync(string url)
        {
            return await httpClient.GetFromJsonAsync<GitHubIssue>($"/api/Issues/by-url?url={url}");
        }

        public async Task<GitHubIssue?> GetIssueByNumberAsync(int issueNumber)
        {
            return await httpClient.GetFromJsonAsync<GitHubIssue>($"/api/Issues/by-issue-number?issueNumber={issueNumber}");
        }

        public async Task<GitHubIssue> CreateIssueAsync(GitHubIssue issue)
        {
            var response = await httpClient.PostAsJsonAsync("/api/Issues", issue);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GitHubIssue>() ?? throw new InvalidOperationException("Failed to create issue.");
        }

        public async Task UpdateIssueAsync(int id, GitHubIssue issue)
        {
            var response = await httpClient.PutAsJsonAsync($"/api/Issues/{id}", issue);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteIssueAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"/api/Issues/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
