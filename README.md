# tree-chopping-dots

Valheim-like style tree chopping system recreated with Unity DOTS.

These are the scripts from my video: https://youtu.be/nFosvi1Yxls

## What I used

- Unity 6.6 (6000.6.3f1)
- Entities 6.6.0
- Entities Graphics 6.6.0
- Unity Physics 6.6.0
- Input System 1.19.0

## How to use it

This repository has only the scripts. There are no models, prefabs or scenes here.

Copy the `Scripts` folder into the `Assets` folder of your project and set up the prefabs the way I do in the video.

A few things that are easy to miss:
- Everything lives in a subscene, so the GameObjects are baked into entities.
- Nothing breaks until the subscene has an object with `ChopEffectsAuthoring` on it. `HitSystem` waits for it.
- A trunk only knocks trees down if "Provides Contacts" is ticked on its collider.
- The player reads the `Move` and `Attack` actions from the project-wide Input System actions.
- The camera follows the object with `CameraTargetAuthoring`, and it moves `Camera.main`.

## The scripts

| File | What it does |
| --- | --- |
| `ChoppableAuthoring.cs` | Health, the hit, and the list of pieces. This goes on a tree, a trunk and a log. |
| `HitSystem.cs` | Applies the hits. When health runs out, replaces the thing with its pieces. |
| `ChopSystem.cs` | Swings the axe and hits the closest choppable thing in front of the player. |
| `CrusherAuthoring.cs`, `CrushSystem.cs` | A falling trunk damages what it lands on. |
| `WobbleSystem.cs` | Shakes a tree that survived a hit. |
| `PlayerAuthoring.cs`, `PlayerSystems.cs` | Input and walking. |
| `PickupAuthoring.cs`, `PickupSystem.cs` | Firewood flies to the player and gets counted. |
| `ScatterAuthoring.cs`, `ScatterSystem.cs` | Fills an area with copies of prefabs. This is how the forest is made. |
| `ChopEffectsAuthoring.cs`, `DebrisAuthoring.cs`, `DebrisSystem.cs` | Wood chips and leaves. |
| `CameraTargetAuthoring.cs`, `CameraFollowSystem.cs` | The camera follows the player. |
| `Hud.cs` | The wood counter. |

## License

MIT. Use the code any way you like.
