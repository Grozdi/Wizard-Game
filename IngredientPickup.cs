using UnityEngine;

/*
    IngredientPickup.cs

    SETUP IN UNITY
    1) Attach this script to your ingredient/loot prefab (for example, BoneDust).
    2) Add a Collider to that prefab and enable "Is Trigger".
    3) Ensure the Player GameObject is tagged as "Player".
    4) Set ingredientName in the Inspector (for example, "BoneDust").

    BEHAVIOR
    - When the Player enters the trigger, this script logs:
      "Picked up [ingredientName]"
    - Then it destroys the ingredient object.
*/

public class IngredientPickup : MonoBehaviour
{
    [Header("Ingredient Settings")]
    [Tooltip("Name of the ingredient shown when picked up.")]
    public string ingredientName = "BoneDust";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log($"Picked up {ingredientName}");
        Destroy(gameObject);
    }
}
