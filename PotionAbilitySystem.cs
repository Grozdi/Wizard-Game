using System.Collections;
using UnityEngine;

/*
    PotionAbilitySystem.cs

    HOW TO USE
    1) Attach this script to the Player GameObject.
    2) Assign PlayerInventory and PlayerController references in the Inspector.
       (If left unassigned, this script will try to find them on the same GameObject in Awake.)
    3) Press key 1 during play mode to consume 1 BoneDust and activate a temporary speed boost.

    EFFECT
    - Consumes 1 BoneDust from inventory.
    - Increases PlayerController.moveSpeed by 50% for 5 seconds.
    - Restores original speed after effect ends.
*/

public class PotionAbilitySystem : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public PlayerController playerController;

    [Header("Speed Potion Settings")]
    [Tooltip("Ingredient required to activate speed potion.")]
    public string speedPotionIngredient = "BoneDust";

    [Tooltip("How much to multiply speed by (1.5 = +50%).")]
    public float speedMultiplier = 1.5f;

    [Tooltip("Duration of speed potion effect in seconds.")]
    public float effectDuration = 5f;

    private Coroutine activeSpeedPotionRoutine;
    private float originalMoveSpeed;
    private bool speedPotionActive;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryActivateSpeedPotion();
        }
    }

    private void TryActivateSpeedPotion()
    {
        if (playerInventory == null || playerController == null)
        {
            Debug.LogWarning("PotionAbilitySystem: Missing PlayerInventory or PlayerController reference.", this);
            return;
        }

        if (!playerInventory.UseIngredient(speedPotionIngredient, 1))
        {
            return;
        }

        if (activeSpeedPotionRoutine != null)
        {
            StopCoroutine(activeSpeedPotionRoutine);

            // If re-activating while active, restore first to avoid stacking errors.
            if (speedPotionActive)
            {
                playerController.moveSpeed = originalMoveSpeed;
                speedPotionActive = false;
            }
        }

        activeSpeedPotionRoutine = StartCoroutine(SpeedPotionRoutine());
    }

    private IEnumerator SpeedPotionRoutine()
    {
        originalMoveSpeed = playerController.moveSpeed;
        playerController.moveSpeed = originalMoveSpeed * speedMultiplier;
        speedPotionActive = true;

        Debug.Log("Speed potion activated");

        yield return new WaitForSeconds(effectDuration);

        playerController.moveSpeed = originalMoveSpeed;
        speedPotionActive = false;
        activeSpeedPotionRoutine = null;

        Debug.Log("Speed potion ended");
    }
}
