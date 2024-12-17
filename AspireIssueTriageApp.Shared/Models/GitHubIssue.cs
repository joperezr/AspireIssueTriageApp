using System.Runtime.CompilerServices;
using Octokit;

namespace AspireIssueTriageApp.Models;

/// <summary>
/// Represents a GitHub issue with various properties.
/// </summary>
public class GitHubIssue : IEquatable<Issue>
{
    /// <summary>
    /// Gets or sets the unique identifier for the issue.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the issue.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the issue.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of the issue.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the milestone associated with the issue.
    /// </summary>
    public string? Milestone { get; set; }

    /// <summary>
    /// Gets or sets the list of labels associated with the issue.
    /// </summary>
    public IList<string> Labels { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of upvotes the issue has received.
    /// </summary>
    public int Upvotes { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the issue is a "how-to" question.
    /// </summary>
    public bool IsHowToQuestion { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the issue is likely a bug.
    /// </summary>
    public bool IsLikelyABug { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the issue is likely a feature request.
    /// </summary>
    public bool IsLikelyAFeatureRequest { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the issue is triaged.
    /// </summary>
    public bool IsTriaged { get; set; } = false;

    /// <summary>
    /// Gets or sets the summary of the issue.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the reasoning for the decision on how to mark the issue.
    /// </summary>
    public string? Reasoning { get; set; }

    public bool Equals(Issue? other)
    {
        if (other == null) return false;

        return this.Title == other.Title &&
               this.Milestone == other.Milestone?.Title &&
               HasSameLabels(other) &&
               Number == other.Number &&
               Upvotes == other.Reactions.Plus1;
    }

    public bool HasAreaLabels()
    {
        return Labels.Any(l => l.StartsWith("area-"));
    }

    private bool HasSameLabels(Issue other)
    {
        if (this.Labels.Count != other.Labels.Count) return false;

        foreach (var label in this.Labels)
        {
            if (!other.Labels.Any(l => l.Name == label))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Issue issue)
        {
            return Equals(issue);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Title, Milestone, Labels, Upvotes);
    }

    public static bool operator ==(GitHubIssue left, Issue right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }

    public static bool operator !=(GitHubIssue left, Issue right)
    {
        return !(left == right);
    }
}