namespace MFCampShared.Messages;

public abstract class WorkflowMessage
{
    public string WorkflowId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}