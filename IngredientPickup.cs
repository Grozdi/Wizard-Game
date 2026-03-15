using UnityEngine;

/*
    IngredientPickup.cs

    SETUP IN UNITY
    1) Attach this script to your ingredient/loot prefab (for example, BoneDust).
    2) Add a Collider to that prefab and enable "Is Trigger".
    3) Ensure the Player GameObject is tagged as "Player".
    4) Ensure the Player GameObject has a PlayerInventory component attached.
    5) Set ingredientName in the Inspector (for example, "Bone Dust").

    BEHAVIOR
    - When the Player enters the trigger, this script calls:
      PlayerInventory.AddIngredient(ingredientName, 1)
    - Logs pickup confirmation including new total quantity.
    - Then destroys the ingredient object.
*/

public class IngredientPickup : MonoBehaviour
{
    [Header("Ingredient Settings")]
    [Tooltip("Name of the ingredient to add to player inventory.")]
    public string ingredientName = "Bone Dust";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
            Debug.LogWarning("IngredientPickup: Player is missing PlayerInventory component.", this);
            return;
        }

        playerInventory.AddIngredient(ingredientName, 1);
        int newAmount = playerInventory.GetIngredientAmount(ingredientName);
        Debug.Log($"Picked up {ingredientName}, now have {newAmount}");

        Destroy(gameObject);
    }
}
