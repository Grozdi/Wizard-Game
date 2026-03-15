# Wizard-Game Version 0.1

This release bundles the current core gameplay scripts needed for a playable prototype loop.

## Included systems
- `PlayerController.cs` - first-person movement, mouse look, projectile shooting, and player health/damage handling.
- `EnemySkeleton.cs` - NavMeshAgent enemy chase AI, contact damage, projectile damage intake, and loot drop on death.
- `EnemySpawner.cs` - coroutine-based enemy spawning with spawn-point selection, interval control, and alive-count cap.
- `LootDropper.cs` - optional chance-based loot spawn helper.
- `IngredientPickup.cs` - trigger pickup logic that adds ingredients to player inventory.
- `PlayerInventory.cs` - ingredient storage and usage APIs.
- `InventoryUI.cs` - TextMeshPro inventory debug display.
- `PotionCraftingSystem.cs` - recipe-based crafting and craftable potion queries.
- `PotionAbilitySystem.cs` - temporary speed potion ability consuming `BoneDust` on `Alpha1`.

## Notes
- This is the baseline "version 0.1" integration snapshot.
- Final gameplay balancing, scene wiring, and QA playtesting are still required.
