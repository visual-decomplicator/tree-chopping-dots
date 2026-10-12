using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// The camera is a normal GameObject, so this system runs on the main thread without Burst.
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct CameraFollowSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        foreach (var (target, transform) in SystemAPI.Query<RefRO<CameraTarget>, RefRO<LocalToWorld>>())
        {
            var focus = transform.ValueRO.Position + math.up() * 1.4f;
            var wanted = focus + target.ValueRO.Offset;
            var blend = 1 - math.exp(-target.ValueRO.Smoothing * SystemAPI.Time.DeltaTime);

            camera.transform.position = math.lerp(camera.transform.position, wanted, blend);
            camera.transform.rotation = quaternion.LookRotation(math.normalize(-target.ValueRO.Offset), math.up());
        }
    }
}
