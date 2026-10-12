using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// Shakes entities whose Wobble is switched on, then puts them back exactly where they were.
public partial struct WobbleSystem : ISystem
{
    const float Duration = 0.45f;
    const float Angle = 0.035f;     // radians
    const float Frequency = 28f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (wobble, enabled, transform) in
                 SystemAPI.Query<RefRW<Wobble>, EnabledRefRW<Wobble>, RefRW<LocalTransform>>())
        {
            wobble.ValueRW.Time += deltaTime;
            var t = wobble.ValueRO.Time / Duration;
            if (t >= 1)
            {
                transform.ValueRW.Rotation = wobble.ValueRO.Rest;
                enabled.ValueRW = false;
                continue;
            }

            var angle = Angle * (1 - t) * math.sin(wobble.ValueRO.Time * Frequency);
            transform.ValueRW.Rotation = math.mul(quaternion.AxisAngle(wobble.ValueRO.Axis, angle), wobble.ValueRO.Rest);
        }
    }
}
