using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

// Swings the axe. At the moment of impact, the closest choppable thing in front of the player gets a Hit.
public partial struct ChopSystem : ISystem
{
    const float ImpactMoment = 0.45f; // fraction of the swing at which the axe lands

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var hits = SystemAPI.GetComponentLookup<Hit>();
        var transforms = SystemAPI.GetComponentLookup<LocalTransform>();

        foreach (var (chopper, input, entity) in
                 SystemAPI.Query<RefRW<Chopper>, RefRO<PlayerInput>>().WithEntityAccess())
        {
            ref var chop = ref chopper.ValueRW;

            if (chop.Timer <= 0 && input.ValueRO.Chop)
            {
                chop.Timer = chop.SwingTime;
                chop.HasHit = false;
            }

            var swing = 0f; // 0 = start of the swing, 1 = end
            if (chop.Timer > 0)
            {
                chop.Timer -= deltaTime;
                swing = 1 - math.saturate(chop.Timer / chop.SwingTime);
            }

            // Pose the axe: wind up, strike, return.
            var axe = transforms[chop.Axe];
            axe.Rotation = AxePose(swing);
            transforms[chop.Axe] = axe;

            if (chop.Timer <= 0 || chop.HasHit || swing < ImpactMoment)
                continue;
            chop.HasHit = true;

            var player = transforms[entity];
            var forward = math.forward(player.Rotation);
            var center = player.Position + math.up() + forward * chop.Reach;

            var found = new NativeList<DistanceHit>(Allocator.Temp);
            world.OverlapSphere(center, chop.Radius, ref found, CollisionFilter.Default);

            // Hit the closest choppable thing.
            var target = Entity.Null;
            var point = center;
            var closest = float.MaxValue;
            foreach (var candidate in found)
            {
                if (candidate.Entity == entity || !hits.HasComponent(candidate.Entity) ||
                    candidate.Distance >= closest)
                    continue;
                closest = candidate.Distance;
                target = candidate.Entity;
                point = candidate.Position;
            }

            if (target == Entity.Null)
                continue;

            hits[target] = new Hit { Damage = chop.Damage, Direction = forward, Point = point };
            hits.SetComponentEnabled(target, true);
        }
    }

    static quaternion AxePose(float swing)
    {
        // Angles in degrees around the pivot: x tilts the axe forward, y sweeps it sideways.
        var rest = new float3(-25, 0, 0);
        var windUp = new float3(-70, 95, 0);
        var strike = new float3(60, -45, 0);

        float3 angles;
        if (swing <= 0)
            angles = rest;
        else if (swing < 0.25f)
            angles = math.lerp(rest, windUp, math.smoothstep(0, 0.25f, swing));
        else if (swing < ImpactMoment)
            angles = math.lerp(windUp, strike, math.smoothstep(0.25f, ImpactMoment, swing));
        else
            angles = math.lerp(strike, rest, math.smoothstep(ImpactMoment + 0.15f, 1, swing));

        return quaternion.Euler(math.radians(angles));
    }
}
