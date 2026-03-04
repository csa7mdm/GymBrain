using GymBrain.Domain.Common;

namespace GymBrain.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public string PayloadJson { get; private set; }
    public bool IsCompleted { get; private set; }

    // EF Core constructor
    private WorkoutSession() { PayloadJson = ""; }

    public WorkoutSession(Guid userId, string payloadJson)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PayloadJson = payloadJson;
        CreatedAtUtc = DateTime.UtcNow;
        IsCompleted = false;
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
    }

    public void UpdatePayload(string newPayloadJson)
    {
        PayloadJson = newPayloadJson;
    }
}
