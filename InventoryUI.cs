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
    5) Assign playerInventory in the Inspector (or let it auto-find in Start).

    BEHAVIOR
    - In Start(), finds PlayerInventory automatically if one is not assigned.
    - In Update(), reads PlayerInventory.ingredients every frame.
    - Displays all ingredient names/quantities, or "(empty)" if inventory has no entries.
*/

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to PlayerInventory. If not assigned, auto-found in Start().")]
    public PlayerInventory playerInventory;

    [Tooltip("TextMeshPro field that renders inventory text.")]
    public TMP_Text inventoryText;

    private string lastRenderedText = string.Empty;
    private int lastTotalItemCount = 0;

    private void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("InventoryUI: No PlayerInventory found in scene.", this);
        }
        else
        {
            Debug.Log("InventoryUI: PlayerInventory found.", this);
        }
    }

    private void Update()
    {
        if (inventoryText == null)
        {
            return;
        }

        string newText = BuildInventoryText();
        inventoryText.text = newText;

        if (newText != lastRenderedText)
        {
            Debug.Log("InventoryUI: UI refreshed.", this);
            lastRenderedText = newText;
        }

        int currentTotalItems = GetTotalItemCount();
        if (currentTotalItems > lastTotalItemCount)
        {
            Debug.Log($"InventoryUI: Items added. Total item quantity is now {currentTotalItems}.", this);
        }

        lastTotalItemCount = currentTotalItems;
    }

    private string BuildInventoryText()
    {
        if (playerInventory == null || playerInventory.ingredients == null || playerInventory.ingredients.Count == 0)
        {
            return "Inventory\n(empty)";
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Inventory");

        foreach (var ingredient in playerInventory.ingredients)
        {
            builder.AppendLine($"{ingredient.Key}: {ingredient.Value}");
        }

        return builder.ToString();
    }

    private int GetTotalItemCount()
    {
        if (playerInventory == null || playerInventory.ingredients == null)
        {
            return 0;
        }

        int total = 0;
        foreach (var ingredient in playerInventory.ingredients)
        {
            total += ingredient.Value;
        }

        return total;
    }
}
