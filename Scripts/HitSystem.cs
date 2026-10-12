using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

// Applies hits. When health runs out, the entity is replaced by its pieces.
[UpdateAfter(typeof(ChopSystem))]
public partial struct HitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ChopEffects>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Played back at the start of the next frame, before transforms are computed. Pieces created at the end
        // of a frame would be drawn once where their prefab stands, in the middle of the world.
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        var effects = SystemAPI.GetSingleton<ChopEffects>();
        var velocities = SystemAPI.GetComponentLookup<PhysicsVelocity>(true);
        var masses = SystemAPI.GetComponentLookup<PhysicsMass>(true);
        var wobbles = SystemAPI.GetComponentLookup<Wobble>();
        var random = Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime * 1000));

        foreach (var (hit, hitEnabled, health, transform, motion, pieces, entity) in
                 SystemAPI.Query<RefRO<Hit>, EnabledRefRW<Hit>, RefRW<Health>, RefRO<LocalTransform>,
                     RefRO<BreakMotion>, DynamicBuffer<BreakPiece>>().WithEntityAccess())
        {
            hitEnabled.ValueRW = false;
            health.ValueRW.Value -= hit.ValueRO.Damage;

            var broken = health.ValueRO.Value <= 0;
            effects.SpawnChips(ref ecb, ref random, hit.ValueRO.Point, hit.ValueRO.Direction,
                broken ? effects.ChipsPerBreak : effects.ChipsPerHit);
            var direction = hit.ValueRO.Direction;
            if (!broken)
            {
                // Still standing: shake it. Things that are already moving don't need it.
                if (!velocities.HasComponent(entity) && !wobbles.IsComponentEnabled(entity))
                {
                    wobbles[entity] = new Wobble
                    {
                        Axis = math.cross(math.up(), direction),
                        Rest = transform.ValueRO.Rotation
                    };
                    wobbles.SetComponentEnabled(entity, true);
                }

                continue;
            }

            var rotation = transform.ValueRO.Rotation;
            var scale = transform.ValueRO.Scale;
            var inherited = velocities.HasComponent(entity) ? velocities[entity].Linear : float3.zero;

            if (motion.ValueRO.Leaves > 0)
                effects.SpawnLeaves(ref ecb, ref random, transform.ValueRO.TransformPoint(motion.ValueRO.Crown),
                    1.6f * scale, motion.ValueRO.Leaves);

            foreach (var piece in pieces)
            {
                var instance = ecb.Instantiate(piece.Prefab);
                var position = transform.ValueRO.TransformPoint(piece.Offset);
                ecb.SetComponent(instance, LocalTransform.FromPositionRotationScale(position, rotation, scale));

                // Static pieces (the stump) have no velocity to set.
                if (!velocities.HasComponent(piece.Prefab))
                    continue;

                // Tip the piece over around its own origin, away from the hit.
                var mass = masses[piece.Prefab];
                var angular = math.cross(math.up(), direction) * motion.ValueRO.Topple;
                var centerOfMass = math.rotate(rotation, mass.CenterOfMass) * scale;
                var velocity = new PhysicsVelocity
                {
                    Linear = inherited + math.cross(angular, centerOfMass)
                             + (random.NextFloat3Direction() + math.up()) * motion.ValueRO.Scatter
                };
                velocity.SetAngularVelocityWorldSpace(mass, rotation, angular);
                ecb.SetComponent(instance, velocity);
            }

            ecb.DestroyEntity(entity);
        }
    }
}
