namespace VideoCombining;

/// <summary>
/// Represents a progress report for a background operation.
/// </summary>
/// <param name="PercentComplete"></param>
/// <param name="Status"></param>
public record ProgressReport(int PercentComplete, string? Status);
