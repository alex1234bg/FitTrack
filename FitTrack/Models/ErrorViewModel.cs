namespace FitTrack.Models;

// Passed to the shared Error view; RequestId is set by ASP.NET when tracing is active.
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
