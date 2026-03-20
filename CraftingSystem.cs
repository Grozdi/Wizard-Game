using System.Collections.Generic;
using UnityEngine;

/*
    CraftingSystem.cs

    HOW TO USE
    1) Attach this script to the Player GameObject (or another manager object).
    2) Assign PlayerInventory in the Inspector, or let the script auto-find it on the same object.
    3) Press:
       - Key 2 to craft SpeedPotion
       - Key 3 to craft StrengthPotion

    NOTES
    - Uses the existing PlayerInventory ingredient system.
    - Keeps the system simple and expandable by using PotionRecipe objects and a recipe lookup table.
    - For now, successful crafting logs the crafted potion name as the result/effect trigger.
*/

public class CraftingSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;

    private readonly Dictionary<string, PotionRecipe> recipes = new Dictionary<string, PotionRecipe>();

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        InitializeRecipes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CraftPotion("SpeedPotion");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CraftPotion("StrengthPotion");
        }
    }

    /// <summary>
    /// Attempts to craft a potion by name using ingredients from PlayerInventory.
    /// </summary>
    public void CraftPotion(string potionName)
    {
        if (playerInventory == null)
        {
            Debug.LogError("CraftingSystem: PlayerInventory reference is missing.", this);
            return;
        }

        if (!recipes.TryGetValue(potionName, out PotionRecipe recipe))
        {
            Debug.LogWarning($"CraftingSystem: Recipe '{potionName}' was not found.", this);
            return;
        }

        if (!HasRequiredIngredients(recipe))
        {
            Debug.Log("Not enough ingredients");
            Debug.Log("Missing ingredients");
            return;
        }

        foreach (KeyValuePair<string, int> ingredient in recipe.ingredientsRequired)
        {
            playerInventory.UseIngredient(ingredient.Key, ingredient.Value);
        }

        // For now, crafting success simply triggers/logs the potion result.
        if (recipe.potionName == "SpeedPotion")
        {
            Debug.Log("Crafted Speed Potion");
        }
        else
        {
            Debug.Log($"Crafted {recipe.potionName}");
        }
    }

    private void InitializeRecipes()
    {
        recipes.Clear();

        PotionRecipe speedPotion = new PotionRecipe("SpeedPotion");
        speedPotion.ingredientsRequired.Add("BoneDust", 1);
        recipes.Add(speedPotion.potionName, speedPotion);

        PotionRecipe strengthPotion = new PotionRecipe("StrengthPotion");
        strengthPotion.ingredientsRequired.Add("BoneDust", 2);
        recipes.Add(strengthPotion.potionName, strengthPotion);
    }

    private bool HasRequiredIngredients(PotionRecipe recipe)
    {
        foreach (KeyValuePair<string, int> ingredient in recipe.ingredientsRequired)
        {
            if (playerInventory.GetIngredientAmount(ingredient.Key) < ingredient.Value)
            {
                return false;
            }
        }

        return true;
    }
}
