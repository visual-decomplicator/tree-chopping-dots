using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

// Reads the Move and Attack actions of the Input System into the PlayerInput component.
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    InputAction move;
    InputAction attack;

    protected override void OnCreate()
    {
        move = InputSystem.actions.FindAction("Move");
        attack = InputSystem.actions.FindAction("Attack");
        move.Enable();
        attack.Enable();
    }

    protected override void OnUpdate()
    {
        float2 direction = move.ReadValue<Vector2>();
        var chop = attack.IsPressed();

        foreach (var input in SystemAPI.Query<RefRW<PlayerInput>>())
        {
            input.ValueRW.Move = direction;
            input.ValueRW.Chop = chop;
        }
    }
}

// Walks the player around. The physics engine keeps it out of the trees.
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct PlayerMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (player, input, velocity, mass, transform) in
                 SystemAPI.Query<RefRO<Player>, RefRO<PlayerInput>, RefRW<PhysicsVelocity>, RefRW<PhysicsMass>,
                     RefRW<LocalTransform>>())
        {
            var move = new float3(input.ValueRO.Move.x, 0, input.ValueRO.Move.y);
            velocity.ValueRW.Linear = new float3(move.x * player.ValueRO.Speed, velocity.ValueRO.Linear.y,
                move.z * player.ValueRO.Speed);

            // Never let physics tip the player over.
            mass.ValueRW.InverseInertia = float3.zero;
            velocity.ValueRW.Angular = float3.zero;

            if (math.lengthsq(move) > 0.01f)
            {
                var target = quaternion.LookRotation(move, math.up());
                transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, target,
                    math.saturate(player.ValueRO.TurnSpeed * deltaTime));
            }
        }
    }
}
