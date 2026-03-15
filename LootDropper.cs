using UnityEngine;

/*
    LootDropper.cs

    HOW TO USE
    1) Attach this script to your enemy GameObject.
    2) Assign a loot prefab in the Inspector (lootPrefab).
    3) Set dropChance between 0 and 1:
       - 0   = never drops
       - 0.5 = 50% chance
       - 1   = always drops
    4) Call DropLoot() when the enemy dies.
       Example from an enemy script's Die() method:
       var lootDropper = GetComponent<LootDropper>();
       if (lootDropper != null) lootDropper.DropLoot();
*/

public class LootDropper : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Prefab to spawn when loot drops.")]
    public GameObject lootPrefab;

    [Range(0f, 1f)]
    [Tooltip("Chance to drop loot (0 = never, 1 = always).")]
    public float dropChance = 0.5f;

    /// <summary>
    /// Attempts to drop loot at this enemy's position based on dropChance.
    /// Call this when the enemy dies.
    /// </summary>
    public void DropLoot()
    {
        if (lootPrefab == null)
        {
            Debug.LogWarning("LootDropper: lootPrefab is not assigned.", this);
            return;
        }

        float roll = Random.value;
        if (roll <= dropChance)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
        }
    }
}
