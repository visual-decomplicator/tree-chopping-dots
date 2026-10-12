using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// A small piece of flying debris, such as a wood chip. No physics, just a simple arc.
public struct Debris : IComponentData
{
    public float3 Velocity;
    public float3 Spin;
    public float Life;
    public float Gravity;
}

public class DebrisAuthoring : MonoBehaviour
{
    class Baker : Baker<DebrisAuthoring>
    {
        public override void Bake(DebrisAuthoring authoring)
        {
            AddComponent<Debris>(GetEntity(TransformUsageFlags.Dynamic));
        }
    }
}
