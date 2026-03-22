using System.Collections.Generic;
using UnityEngine;

/*
    PlayerInventory.cs

    HOW TO USE
    1) Attach this script to your Player GameObject.
    2) Call AddIngredient(...) when the player picks up ingredients.
    3) Call UseIngredient(...) when crafting/consuming ingredients.
    4) Call GetIngredientAmount(...) to check current quantity for UI/gameplay.
    5) Call HasItem(...) to validate ingredient quantity before crafting.
*/

public class PlayerInventory : MonoBehaviour
{
    [Header("Ingredient Inventory (Runtime)")]
    [Tooltip("Tracks ingredient names and their quantities at runtime.")]
    public Dictionary<string, int> ingredients = new Dictionary<string, int>();

    public void AddIngredient(string ingredientName, int amount)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            Debug.LogWarning("PlayerInventory: ingredientName is null/empty. AddIngredient ignored.", this);
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"PlayerInventory: amount must be > 0 to add. Received {amount}.", this);
            return;
        }

        if (ingredients.ContainsKey(ingredientName))
        {
            ingredients[ingredientName] += amount;
        }
        else
        {
            ingredients.Add(ingredientName, amount);
        }

        Debug.Log($"PlayerInventory: Added {amount} x {ingredientName}. Total: {ingredients[ingredientName]}", this);
    }

    public int GetIngredientAmount(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            return 0;
        }

        if (ingredients.TryGetValue(ingredientName, out int amount))
        {
            return amount;
        }

        return 0;
    }

    /// <summary>
    /// Returns true only if the inventory contains at least the requested amount.
    /// </summary>
    public bool HasItem(string ingredientName, int amount)
    {
        if (string.IsNullOrWhiteSpace(ingredientName) || amount <= 0)
        {
            return false;
        }

        return GetIngredientAmount(ingredientName) >= amount;
    }

    public bool UseIngredient(string ingredientName, int amount)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            Debug.LogWarning("PlayerInventory: ingredientName is null/empty. UseIngredient failed.", this);
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"PlayerInventory: amount must be > 0 to use. Received {amount}.", this);
            return false;
        }

        int currentAmount = GetIngredientAmount(ingredientName);
        if (currentAmount < amount)
        {
            Debug.Log($"PlayerInventory: Not enough {ingredientName}. Needed {amount}, have {currentAmount}.", this);
            return false;
        }

        ingredients[ingredientName] = currentAmount - amount;
        Debug.Log($"PlayerInventory: Used {amount} x {ingredientName}. Remaining: {ingredients[ingredientName]}", this);

        if (ingredients[ingredientName] <= 0)
        {
            ingredients.Remove(ingredientName);
        }

        return true;
    }
}
