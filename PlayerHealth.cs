using UnityEngine;

/*
    PlayerHealth.cs

    Basic standalone player health component for enemy attacks and future UI integration.
*/
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Current health: {currentHealth}", this);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("PlayerHealth: Player died.", this);
        }
    }
}
