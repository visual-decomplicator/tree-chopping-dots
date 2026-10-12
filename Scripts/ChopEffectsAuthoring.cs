using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Singleton with the prefabs used for chop effects.
public struct ChopEffects : IComponentData
{
    public Entity Chip;
    public Entity Leaf;
    public int ChipsPerHit;
    public int ChipsPerBreak;

    // Wood chips fly back towards whoever made the hit.
    public void SpawnChips(ref EntityCommandBuffer ecb, ref Unity.Mathematics.Random random,
        float3 point, float3 direction, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var velocity = -direction * random.NextFloat(1f, 3f)
                           + random.NextFloat3Direction() * 1.5f
                           + math.up() * random.NextFloat(2f, 4f);
            Spawn(ref ecb, ref random, Chip, point, velocity, random.NextFloat(1.5f, 3f),
                random.NextFloat(0.8f, 1.6f), 9.81f);
        }
    }

    // Leaves burst out of the crown and drift down.
    public void SpawnLeaves(ref EntityCommandBuffer ecb, ref Unity.Mathematics.Random random,
        float3 point, float radius, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var offset = random.NextFloat3Direction() * random.NextFloat(radius);
            var velocity = math.normalizesafe(offset) * random.NextFloat(1f, 3.5f)
                           + math.up() * random.NextFloat(0.5f, 2.5f);
            Spawn(ref ecb, ref random, Leaf, point + offset, velocity, random.NextFloat(2f, 4f),
                random.NextFloat(1.5f, 3f), 2.2f);
        }
    }

    void Spawn(ref EntityCommandBuffer ecb, ref Unity.Mathematics.Random random, Entity prefab,
        float3 position, float3 velocity, float scale, float life, float gravity)
    {
        var instance = ecb.Instantiate(prefab);
        ecb.SetComponent(instance,
            LocalTransform.FromPositionRotationScale(position, random.NextQuaternionRotation(), scale));
        ecb.SetComponent(instance, new Debris
        {
            Velocity = velocity,
            Spin = random.NextFloat3(-10f, 10f),
            Life = life,
            Gravity = gravity
        });
    }
}

public class ChopEffectsAuthoring : MonoBehaviour
{
    public GameObject chip;
    public GameObject leaf;
    public int chipsPerHit = 6;
    public int chipsPerBreak = 16;

    class Baker : Baker<ChopEffectsAuthoring>
    {
        public override void Bake(ChopEffectsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ChopEffects
            {
                Chip = GetEntity(authoring.chip, TransformUsageFlags.Dynamic),
                Leaf = GetEntity(authoring.leaf, TransformUsageFlags.Dynamic),
                ChipsPerHit = authoring.chipsPerHit,
                ChipsPerBreak = authoring.chipsPerBreak
            });
        }
    }
}
