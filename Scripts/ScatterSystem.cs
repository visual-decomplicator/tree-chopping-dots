using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// Runs once: places all the copies, then removes the Scatter entity.
public partial struct ScatterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Scatter>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var query = SystemAPI.QueryBuilder().WithAll<Scatter, ScatterPrefab>().Build();
        var scatterEntities = query.ToEntityArray(Allocator.Temp);

        foreach (var scatterEntity in scatterEntities)
        {
            var scatter = SystemAPI.GetComponent<Scatter>(scatterEntity);
            var prefabs = SystemAPI.GetBuffer<ScatterPrefab>(scatterEntity).ToNativeArray(Allocator.Temp);
            var random = new Unity.Mathematics.Random(scatter.Seed);

            // Lay the copies out on a grid, then nudge each one so the grid doesn't show.
            var side = (int)math.ceil(math.sqrt(scatter.Count));
            var origin = -0.5f * side * scatter.Spacing;

            for (var i = 0; i < scatter.Count; i++)
            {
                var cell = new float2(i % side, i / side) + random.NextFloat2(0.1f, 0.9f);
                var position = new float3(origin + cell.x * scatter.Spacing, 0, origin + cell.y * scatter.Spacing);
                var rotation = quaternion.RotateY(random.NextFloat(math.PI * 2));
                var scale = random.NextFloat(scatter.Scale.x, scatter.Scale.y);
                var prefab = prefabs[random.NextInt(prefabs.Length)].Prefab;

                if (math.length(position) < scatter.Clearing)
                    continue;

                var instance = state.EntityManager.Instantiate(prefab);
                SystemAPI.SetComponent(instance, LocalTransform.FromPositionRotationScale(position, rotation, scale));
            }

            state.EntityManager.DestroyEntity(scatterEntity);
        }
    }
}
