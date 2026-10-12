using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Fills a square area with copies of some prefabs: the forest, the grass, the rocks.
public struct Scatter : IComponentData
{
    public int Count;
    public float Spacing;     // average distance between neighbours
    public float Clearing;    // nothing is placed this close to the centre
    public float2 Scale;      // min and max size
    public uint Seed;
}

public struct ScatterPrefab : IBufferElementData
{
    public Entity Prefab;
}

public class ScatterAuthoring : MonoBehaviour
{
    public GameObject[] prefabs;
    public int count = 10000;
    public float spacing = 4;
    public float clearing = 6;
    public Vector2 scale = new Vector2(0.8f, 1.3f);
    public uint seed = 1;

    class Baker : Baker<ScatterAuthoring>
    {
        public override void Bake(ScatterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Scatter
            {
                Count = authoring.count,
                Spacing = authoring.spacing,
                Clearing = authoring.clearing,
                Scale = authoring.scale,
                Seed = authoring.seed
            });
            var prefabs = AddBuffer<ScatterPrefab>(entity);
            foreach (var prefab in authoring.prefabs)
                prefabs.Add(new ScatterPrefab { Prefab = GetEntity(prefab, TransformUsageFlags.Dynamic) });
        }
    }
}
