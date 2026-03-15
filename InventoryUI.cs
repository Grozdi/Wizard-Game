using System.Text;
using UnityEngine;
using TMPro;

/*
    InventoryUI.cs

    HOW TO SET UP IN UNITY
    1) Create a Canvas in your scene.
    2) Add a TextMeshPro UI element (GameObject > UI > Text - TextMeshPro).
    3) Attach this script to any GameObject (for example, Canvas or UI Manager).
    4) Assign inventoryText in the Inspector to the TMP_Text component.
       (playerInventory is auto-found in Start.)

    BEHAVIOR
    - In Start(), finds PlayerInventory automatically.
    - In Update(), reads PlayerInventory.ingredients every frame.
    - Shows "(empty)" only when there are no items.
*/

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Auto-found at runtime in Start().")]
    public PlayerInventory playerInventory;

    [Tooltip("TextMeshPro text field used to display inventory contents.")]
    public TMP_Text inventoryText;

    private void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();

        if (playerInventory == null)
        {
            Debug.LogWarning("InventoryUI: No PlayerInventory found in scene.", this);
        }
        else
        {
            Debug.Log("InventoryUI: PlayerInventory found successfully.", this);
        }
    }

    private void Update()
    {
        if (inventoryText == null)
        {
            return;
        }

        if (playerInventory == null || playerInventory.ingredients == null)
        {
            inventoryText.text = "Inventory\n(empty)";
            return;
        }

        if (playerInventory.ingredients.Count == 0)
        {
            inventoryText.text = "Inventory\n(empty)";
            Debug.Log("InventoryUI: Inventory is currently empty.", this);
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Inventory");

        foreach (var ingredient in playerInventory.ingredients)
        {
            builder.AppendLine($"{ingredient.Key}: {ingredient.Value}");
        }

        inventoryText.text = builder.ToString();
        Debug.Log($"InventoryUI: Displaying {playerInventory.ingredients.Count} inventory item type(s).", this);
    }
}
