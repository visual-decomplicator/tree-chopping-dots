using Unity.Entities;
using UnityEngine;

// Something heavy that damages whatever it falls on: a falling trunk.
public struct Crusher : IComponentData
{
    public float Damage;
    public float MinSpeed;   // slower than this, it does no damage
}

public class CrusherAuthoring : MonoBehaviour
{
    public float damage = 3;
    public float minSpeed = 2f;

    class Baker : Baker<CrusherAuthoring>
    {
        public override void Bake(CrusherAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic),
                new Crusher { Damage = authoring.damage, MinSpeed = authoring.minSpeed });
        }
    }
}
