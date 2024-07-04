namespace SampleWebApp.Tests;

public class ValidationResult
{
    public required string Title { get; set; }
    public int Status { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
