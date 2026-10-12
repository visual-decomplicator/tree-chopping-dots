using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

// Shows the wood count. A normal MonoBehaviour that reads one value from the entity world.
public class Hud : MonoBehaviour
{
    public Text woodLabel;

    EntityQuery inventoryQuery;

    void Start()
    {
        inventoryQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(Inventory));
    }

    void Update()
    {
        if (inventoryQuery.TryGetSingleton<Inventory>(out var inventory))
            woodLabel.text = "Wood  " + inventory.Wood;
    }
}
