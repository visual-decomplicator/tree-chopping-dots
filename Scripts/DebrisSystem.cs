using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// Moves the debris along its arc and removes it when its time is up.
public partial struct DebrisSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        new DebrisJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            Ecb = ecb.AsParallelWriter()
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct DebrisJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref Debris debris,
            ref LocalTransform transform)
        {
            debris.Life -= DeltaTime;
            if (debris.Life <= 0)
            {
                Ecb.DestroyEntity(chunkIndex, entity);
                return;
            }

            // Fly until it lands, then lie still.
            if (transform.Position.y > 0.03f || debris.Velocity.y > 0)
            {
                debris.Velocity.y -= debris.Gravity * DeltaTime;
                transform.Position += debris.Velocity * DeltaTime;
                transform.Rotation = math.mul(transform.Rotation, quaternion.Euler(debris.Spin * DeltaTime));
            }

            // Shrink away during the last quarter second.
            if (debris.Life < 0.25f)
                transform.Scale *= math.saturate(1 - DeltaTime / debris.Life);
        }
    }
}
