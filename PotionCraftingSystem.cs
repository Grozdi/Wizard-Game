using System;
using System.Collections.Generic;
using UnityEngine;

/*
    PotionCraftingSystem.cs

    HOW TO SET UP RECIPES IN THE INSPECTOR
    1) Attach this script to a scene object (for example, "CraftingManager").
    2) In the Recipes list, add one element per potion.
    3) For each recipe:
       - potionName: display/lookup name of the potion (example: "Health Potion").
       - requiredIngredients: list every required ingredient name.
         (You can repeat names for quantity, e.g. ["Herb", "Herb", "Water"] = 2 Herb + 1 Water.)
       - potionPrefab: prefab to spawn when crafted.

    HOW TO CALL TryCraftPotion (EXAMPLE VIA UI BUTTON)
    - Hook your button click to a method like:
        craftingSystem.TryCraftPotion("Health Potion", playerInventory);
    - On success, ingredients are consumed and potionPrefab is instantiated at player position.

    HOW TO USE GetCraftablePotions FOR UI
    - Call:
        List<string> craftable = craftingSystem.GetCraftablePotions(playerInventory);
    - Use this list to enable/disable buttons or populate a craftable list in your UI.
*/

[Serializable]
public class PotionRecipe
{
    public string potionName;
    public List<string> requiredIngredients = new List<string>();
    public GameObject potionPrefab;
}

public class PotionCraftingSystem : MonoBehaviour
{
    [Header("Potion Recipes")]
    [Tooltip("All craftable potion recipes configured in the Inspector.")]
    public List<PotionRecipe> recipes = new List<PotionRecipe>();

    /// <summary>
    /// Attempts to craft a potion by name using the provided player inventory.
    /// Returns true if crafting succeeds; otherwise false.
    /// </summary>
    public bool TryCraftPotion(string potionName, PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogWarning("PotionCraftingSystem: inventory is null.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(potionName))
        {
            Debug.LogWarning("PotionCraftingSystem: potionName is empty.", this);
            return false;
        }

        PotionRecipe recipe = FindRecipe(potionName);
        if (recipe == null)
        {
            Debug.LogWarning($"PotionCraftingSystem: No recipe found for '{potionName}'.", this);
            return false;
        }

        if (recipe.potionPrefab == null)
        {
            Debug.LogWarning($"PotionCraftingSystem: Recipe '{recipe.potionName}' has no potionPrefab assigned.", this);
            return false;
        }

        Dictionary<string, int> requiredCounts = BuildIngredientCounts(recipe.requiredIngredients);

        // Validate inventory has everything required before consuming any ingredient.
        foreach (KeyValuePair<string, int> pair in requiredCounts)
        {
            int available = inventory.GetIngredientAmount(pair.Key);
            if (available < pair.Value)
            {
                Debug.Log($"PotionCraftingSystem: Missing ingredient '{pair.Key}'. Need {pair.Value}, have {available}.", this);
                return false;
            }
        }

        // Consume ingredients now that validation passed.
        foreach (KeyValuePair<string, int> pair in requiredCounts)
        {
            bool used = inventory.UseIngredient(pair.Key, pair.Value);
            if (!used)
            {
                // Should not happen due to pre-check, but safely abort if inventory changed unexpectedly.
                Debug.LogWarning($"PotionCraftingSystem: Failed consuming '{pair.Key}' x{pair.Value}.", this);
                return false;
            }
        }

        Instantiate(recipe.potionPrefab, inventory.transform.position, Quaternion.identity);
        Debug.Log($"PotionCraftingSystem: Crafted '{recipe.potionName}'.", this);
        return true;
    }

    /// <summary>
    /// Returns potion names that can currently be crafted using the provided inventory.
    /// </summary>
    public List<string> GetCraftablePotions(PlayerInventory inventory)
    {
        List<string> craftable = new List<string>();

        if (inventory == null)
        {
            return craftable;
        }

        foreach (PotionRecipe recipe in recipes)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.potionName))
            {
                continue;
            }

            Dictionary<string, int> requiredCounts = BuildIngredientCounts(recipe.requiredIngredients);
            bool canCraft = true;

            foreach (KeyValuePair<string, int> pair in requiredCounts)
            {
                if (inventory.GetIngredientAmount(pair.Key) < pair.Value)
                {
                    canCraft = false;
                    break;
                }
            }

            if (canCraft)
            {
                craftable.Add(recipe.potionName);
            }
        }

        return craftable;
    }

    private PotionRecipe FindRecipe(string potionName)
    {
        for (int i = 0; i < recipes.Count; i++)
        {
            PotionRecipe recipe = recipes[i];
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.potionName))
            {
                continue;
            }

            if (string.Equals(recipe.potionName, potionName, StringComparison.OrdinalIgnoreCase))
            {
                return recipe;
            }
        }

        return null;
    }

    private Dictionary<string, int> BuildIngredientCounts(List<string> ingredients)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();

        if (ingredients == null)
        {
            return counts;
        }

        for (int i = 0; i < ingredients.Count; i++)
        {
            string ingredient = ingredients[i];
            if (string.IsNullOrWhiteSpace(ingredient))
            {
                continue;
            }

            if (counts.ContainsKey(ingredient))
            {
                counts[ingredient]++;
            }
            else
            {
                counts.Add(ingredient, 1);
            }
        }

        return counts;
    }
}
