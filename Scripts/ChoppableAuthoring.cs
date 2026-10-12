using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// How much damage an entity can take before it breaks.
public struct Health : IComponentData
{
    public float Value;
}

// Switched on by whatever hits the entity (the axe, a falling trunk).
// HitSystem reads it, applies the damage and switches it off again.
public struct Hit : IComponentData, IEnableableComponent
{
    public float Damage;
    public float3 Direction;
    public float3 Point;
}

// What the entity turns into when its health runs out.
// Tree -> stump + trunk, trunk -> logs, log -> firewood.
public struct BreakPiece : IBufferElementData
{
    public Entity Prefab;
    public float3 Offset;
}

// How the pieces start moving.
public struct BreakMotion : IComponentData
{
    public float Topple;   // rad/s, tips the piece over, away from the hit
    public float Scatter;  // m/s, throws pieces outwards
    public int Leaves;     // leaves to shake loose when it breaks
    public float3 Crown;   // where the leaves come from
}

// Switched on by HitSystem when the entity survives a hit: it shakes for a moment.
public struct Wobble : IComponentData, IEnableableComponent
{
    public float Time;
    public float3 Axis;
    public quaternion Rest;
}

public class ChoppableAuthoring : MonoBehaviour
{
    [Serializable]
    public struct Piece
    {
        public GameObject prefab;
        public Vector3 offset;
    }

    public float health = 3;
    public Piece[] pieces;
    public float topple;
    public float scatter;
    public int leaves;
    public Vector3 crown;

    class Baker : Baker<ChoppableAuthoring>
    {
        public override void Bake(ChoppableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health { Value = authoring.health });
            AddComponent<Hit>(entity);
            SetComponentEnabled<Hit>(entity, false);
            AddComponent(entity, new BreakMotion
            {
                Topple = authoring.topple,
                Scatter = authoring.scatter,
                Leaves = authoring.leaves,
                Crown = authoring.crown
            });
            AddComponent<Wobble>(entity);
            SetComponentEnabled<Wobble>(entity, false);

            var pieces = AddBuffer<BreakPiece>(entity);
            foreach (var piece in authoring.pieces)
            {
                pieces.Add(new BreakPiece
                {
                    Prefab = GetEntity(piece.prefab, TransformUsageFlags.Dynamic),
                    Offset = piece.offset
                });
            }
        }
    }
}
