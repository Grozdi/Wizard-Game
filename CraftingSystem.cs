using System.Collections.Generic;
using UnityEngine;

/*
    CraftingSystem.cs

    HOW TO USE
    1) Attach this script to the Player GameObject (or another manager object).
    2) Assign PlayerInventory in the Inspector, or let the script auto-find it on the same object.
    3) Call CraftPotion(...) from gameplay/UI code, or use the default test keys:
       - Key 2 to craft HealthPotion
       - Key 3 to craft ManaPotion
       - Key 4 to craft StrengthPotion

    NOTES
    - Uses the existing PlayerInventory ingredient system.
    - Ingredients are string-based, so Mushroom, Crystal, FireEssence, and Water work by name.
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
            CraftPotion("HealthPotion");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CraftPotion("ManaPotion");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CraftPotion("StrengthPotion");
        }
    }

    /// <summary>
    /// Attempts to craft a potion by name using ingredients from PlayerInventory.
    /// </summary>
    public void CraftPotion(string potionName)
    {
        Debug.Log($"Crafting started for {potionName}", this);

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

        Debug.Log($"Recipe matched: {recipe.potionName}", this);

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

        Debug.Log($"Crafted {FormatPotionName(recipe.potionName)}", this);
    }

    private void InitializeRecipes()
    {
        recipes.Clear();

        RegisterRecipe("HealthPotion", new Dictionary<string, int>
        {
            { "BoneDust", 1 }
        });

        RegisterRecipe("SpeedPotion", new Dictionary<string, int>
        {
            { "BoneDust", 1 }
        });

        RegisterRecipe("ManaPotion", new Dictionary<string, int>
        {
            { "Water", 1 },
            { "Crystal", 1 }
        });

        RegisterRecipe("StrengthPotion", new Dictionary<string, int>
        {
            { "Mushroom", 1 },
            { "FireEssence", 1 }
        });
    }

    private void RegisterRecipe(string potionName, Dictionary<string, int> ingredients)
    {
        PotionRecipe recipe = new PotionRecipe(potionName);
        foreach (KeyValuePair<string, int> ingredient in ingredients)
        {
            recipe.ingredientsRequired.Add(ingredient.Key, ingredient.Value);
        }

        recipes[potionName] = recipe;
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

    private string FormatPotionName(string potionName)
    {
        if (potionName == "HealthPotion")
        {
            return "Health Potion";
        }

        if (potionName == "ManaPotion")
        {
            return "Mana Potion";
        }

        if (potionName == "StrengthPotion")
        {
            return "Strength Potion";
        }

        if (potionName == "SpeedPotion")
        {
            return "Speed Potion";
        }

        return potionName;
    }
}
