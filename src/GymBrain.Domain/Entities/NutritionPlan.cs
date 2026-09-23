using GymBrain.Domain.Common;

namespace GymBrain.Domain.Entities;

public sealed class NutritionPlan : BaseEntity
{
    private NutritionPlan() { PayloadJson = ""; }

    public NutritionPlan(Guid userId, string payloadJson)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PayloadJson = payloadJson;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string PayloadJson { get; private set; }
}
