namespace MFCampShared.Messages;

public abstract class WorkflowMessage
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}