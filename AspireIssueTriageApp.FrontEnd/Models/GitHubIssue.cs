using System.ComponentModel.DataAnnotations.Schema;

namespace AspireIssueTriageApp.FrontEnd.Models
{
    /// <summary>
    /// Represents a GitHub issue with various properties.
    /// </summary>
    public class GitHubIssue
    {
        /// <summary>
        /// Gets or sets the unique identifier for the issue.
        /// </summary>
        public int Id { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the URL of the issue.
        /// </summary>
        public string Url { get; set; }

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
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the reasoning for the decision on how to mark the issue.
        /// </summary>
        public string Reasoning { get; set; }
    }

}
