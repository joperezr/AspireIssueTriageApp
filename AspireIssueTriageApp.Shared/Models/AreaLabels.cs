using System;

namespace AspireIssueTriageApp.Models;

public record AreaLabel
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
};
