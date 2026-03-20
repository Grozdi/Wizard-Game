using System.Collections.Generic;

/// <summary>
/// Defines a potion recipe by name and the ingredient quantities required to craft it.
/// </summary>
[System.Serializable]
public class PotionRecipe
{
    public string potionName;
    public Dictionary<string, int> ingredientsRequired = new Dictionary<string, int>();

    public PotionRecipe(string potionName)
    {
        this.potionName = potionName;
    }
}
