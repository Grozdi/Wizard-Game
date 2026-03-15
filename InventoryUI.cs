using System.Text;
using UnityEngine;
using TMPro;

/*
    InventoryUI.cs

    HOW TO SET UP IN UNITY
    1) Create a Canvas in your scene.
    2) Add a TextMeshPro UI element (GameObject > UI > Text - TextMeshPro).
    3) Attach this script to any GameObject (for example, Canvas or UI Manager).
    4) In the Inspector, assign:
       - playerInventory: the PlayerInventory component on your Player
       - inventoryText: the TMP_Text component you want to display inventory in

    BEHAVIOR
    - Every frame (Update), this script reads PlayerInventory.ingredients
      and prints all ingredient names with quantities.
*/

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's PlayerInventory component.")]
    public PlayerInventory playerInventory;

    [Tooltip("TextMeshPro text field used to display inventory contents.")]
    public TMP_Text inventoryText;

    private void Update()
    {
        if (inventoryText == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Inventory");

        if (playerInventory == null || playerInventory.ingredients == null)
        {
            inventoryText.text = builder.ToString();
            return;
        }

        foreach (var ingredient in playerInventory.ingredients)
        {
            builder.AppendLine($"{ingredient.Key}: {ingredient.Value}");
        }

        inventoryText.text = builder.ToString();
    }
}
