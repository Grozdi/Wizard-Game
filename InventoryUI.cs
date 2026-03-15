using System.Text;
using UnityEngine;
using UnityEngine.UI;

/*
    InventoryUI.cs

    HOW TO SET UP IN UNITY
    1) Create a Canvas in your scene (GameObject > UI > Canvas).
    2) Add a UI Text element as a child of the Canvas (GameObject > UI > Text).
       (If using TextMeshPro instead, this script expects UnityEngine.UI.Text specifically.)
    3) Attach this InventoryUI script to any GameObject in the scene (for example, the Canvas).
    4) Assign:
       - playerInventory: the Player GameObject that has PlayerInventory.cs
       - inventoryText: the UI Text component that should display inventory contents

    BEHAVIOR
    - Every frame, the script reads PlayerInventory's dictionary and prints all ingredients + quantities.
*/

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's PlayerInventory component.")]
    public PlayerInventory playerInventory;

    [Tooltip("UI Text used to display inventory contents.")]
    public Text inventoryText;

    private void Update()
    {
        if (inventoryText == null)
        {
            return;
        }

        if (playerInventory == null || playerInventory.ingredients == null)
        {
            inventoryText.text = "Inventory: (none)";
            return;
        }

        if (playerInventory.ingredients.Count == 0)
        {
            inventoryText.text = "Inventory: (empty)";
            return;
        }

        StringBuilder builder = new StringBuilder();

        foreach (var entry in playerInventory.ingredients)
        {
            builder.AppendLine($"{entry.Key}: {entry.Value}");
        }

        inventoryText.text = builder.ToString();
    }
}
