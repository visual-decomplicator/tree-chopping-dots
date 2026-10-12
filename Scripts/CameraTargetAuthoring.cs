using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Put on the player: the main camera follows it from this offset.
public struct CameraTarget : IComponentData
{
    public float3 Offset;
    public float Smoothing;
}

public class CameraTargetAuthoring : MonoBehaviour
{
    public Vector3 offset = new Vector3(3.6f, 2.6f, -5.2f);   // behind, to the side and above: what the player chops stays in view
    public float smoothing = 6;

    class Baker : Baker<CameraTargetAuthoring>
    {
        public override void Bake(CameraTargetAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic),
                new CameraTarget { Offset = authoring.offset, Smoothing = authoring.smoothing });
        }
    }
}
