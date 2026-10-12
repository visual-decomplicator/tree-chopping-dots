using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

// Pulls firewood towards the player and counts it.
public partial struct PickupSystem : ISystem
{
    const float MagnetRadius = 3.5f;
    const float CollectRadius = 1.3f;
    const float MagnetSpeed = 9f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (inventory, playerTransform) in SystemAPI.Query<RefRW<Inventory>, RefRO<LocalTransform>>())
        {
            var target = playerTransform.ValueRO.Position + math.up();

            foreach (var (pickup, transform, velocity, entity) in
                     SystemAPI.Query<RefRW<Pickup>, RefRO<LocalTransform>, RefRW<PhysicsVelocity>>()
                         .WithEntityAccess())
            {
                pickup.ValueRW.Delay -= deltaTime;
                if (pickup.ValueRO.Delay > 0)
                    continue;

                var offset = target - transform.ValueRO.Position;
                var distance = math.length(offset);

                if (distance < CollectRadius)
                {
                    inventory.ValueRW.Wood++;
                    ecb.DestroyEntity(entity);
                }
                else if (distance < MagnetRadius)
                {
                    velocity.ValueRW.Linear = offset / distance * MagnetSpeed;
                }
            }
        }
    }
}
