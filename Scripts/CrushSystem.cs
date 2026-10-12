using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

// Turns physics collisions into Hits. The colliders must have "Provides Contacts" ticked.
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct CrushSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new CrushJob
        {
            Crushers = SystemAPI.GetComponentLookup<Crusher>(true),
            Velocities = SystemAPI.GetComponentLookup<PhysicsVelocity>(true),
            Transforms = SystemAPI.GetComponentLookup<LocalTransform>(true),
            Hits = SystemAPI.GetComponentLookup<Hit>()
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    struct CrushJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<Crusher> Crushers;
        [ReadOnly] public ComponentLookup<PhysicsVelocity> Velocities;
        [ReadOnly] public ComponentLookup<LocalTransform> Transforms;
        public ComponentLookup<Hit> Hits;

        public void Execute(CollisionEvent collision)
        {
            Crush(collision.EntityA, collision.EntityB);
            Crush(collision.EntityB, collision.EntityA);
        }

        void Crush(Entity crusher, Entity victim)
        {
            if (!Crushers.TryGetComponent(crusher, out var settings) || !Hits.HasComponent(victim))
                return;

            // Only things that stand still get crushed: trees, not other falling trunks.
            if (Velocities.HasComponent(victim))
                return;

            var velocity = Velocities[crusher].Linear;
            if (math.length(velocity) < settings.MinSpeed)
                return;

            // The victim is pushed away from whatever fell on it.
            var push = Transforms[victim].Position - Transforms[crusher].Position;
            Hits[victim] = new Hit
            {
                Damage = settings.Damage,
                Direction = math.normalizesafe(new float3(push.x, 0, push.z), math.forward()),
                Point = Transforms[victim].Position + math.up()
            };
            Hits.SetComponentEnabled(victim, true);
        }
    }
}
