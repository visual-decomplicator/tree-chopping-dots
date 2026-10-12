using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Player : IComponentData
{
    public float Speed;
    public float TurnSpeed;
}

// What the player wants to do this frame. Written by PlayerInputSystem.
public struct PlayerInput : IComponentData
{
    public float2 Move;
    public bool Chop;
}

public struct Inventory : IComponentData
{
    public int Wood;
}

public struct Chopper : IComponentData
{
    public Entity Axe;       // the pivot entity that swings
    public float Damage;
    public float Reach;      // how far in front of the player the axe lands
    public float Radius;     // how forgiving the hit is
    public float SwingTime;
    public float Timer;      // counts down while swinging
    public bool HasHit;
}

public class PlayerAuthoring : MonoBehaviour
{
    public float speed = 5;
    public float turnSpeed = 12;
    public GameObject axePivot;
    public float damage = 1;
    public float reach = 0.8f;
    public float radius = 0.75f;
    public float swingTime = 0.55f;

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Player { Speed = authoring.speed, TurnSpeed = authoring.turnSpeed });
            AddComponent<PlayerInput>(entity);
            AddComponent<Inventory>(entity);
            AddComponent(entity, new Chopper
            {
                Axe = GetEntity(authoring.axePivot, TransformUsageFlags.Dynamic),
                Damage = authoring.damage,
                Reach = authoring.reach,
                Radius = authoring.radius,
                SwingTime = authoring.swingTime
            });
        }
    }
}
