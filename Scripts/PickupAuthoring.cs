using Unity.Entities;
using UnityEngine;

// Firewood that flies to the player and is added to the inventory.
public struct Pickup : IComponentData
{
    public float Delay;   // seconds before it can be collected
}

public class PickupAuthoring : MonoBehaviour
{
    public float delay = 1.2f;

    class Baker : Baker<PickupAuthoring>
    {
        public override void Bake(PickupAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Pickup { Delay = authoring.delay });
        }
    }
}
