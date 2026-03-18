using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
    EnemySkeleton.cs

    SETUP IN UNITY EDITOR
    1) Attach this script to your enemy prefab (for example, Skeleton).
    2) Add a NavMeshAgent component to the same GameObject.
    3) Add a Collider to the enemy:
       - Use trigger OR collision-based interaction depending on your setup.
       - Ensure physics settings allow contact with player/projectiles.
    4) Ensure your Player GameObject has:
       - Tag: "Player"
       - PlayerController component (for TakeDamage calls)
    5) (Optional) Tag your projectile objects as "Projectile".
    6) Assign lootPrefab in the Inspector if you want this enemy to drop loot on death.
    7) Make sure the enemy has one or more Renderers so hit flash feedback can change material colors.

    LOOT SETUP
    - Drag your loot prefab (e.g., BoneDust pickup) into the "Loot Prefab" field.
    - If lootPrefab is null, no loot is dropped.

    HIT FEEDBACK
    - Normal damage flashes red and scales the enemy to 1.1x briefly.
    - Critical damage flashes yellow and scales the enemy to 1.2x briefly.
    - Use TakeDamage(amount) for normal hits.
    - Use TakeCriticalDamage(amount) if you want the stronger feedback effect.
*/

[RequireComponent(typeof(NavMeshAgent))]
public class EnemySkeleton : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("NavMeshAgent movement speed.")]
    public float moveSpeed = 3.5f;

    [Header("Combat")]
    [Tooltip("Damage dealt to the player on contact.")]
    public float damage = 10f;

    [Tooltip("Minimum time between damage ticks while touching the player.")]
    public float damageCooldown = 0.75f;

    [Tooltip("Damage taken when hit by an object tagged 'Projectile'.")]
    public float projectileDamage = 25f;

    [Header("Health")]
    [Tooltip("Maximum enemy health.")]
    public float maxHealth = 50f;

    [Tooltip("Current enemy health.")]
    public float currentHealth = 50f;

    [Header("Loot")]
    [Tooltip("Loot prefab to spawn when this enemy dies. Leave null for no drop.")]
    public GameObject lootPrefab;

    [Header("Hit Feedback")]
    [Tooltip("How long hit feedback lasts in seconds.")]
    public float hitFeedbackDuration = 0.1f;

    [Tooltip("Tint color used for normal hits.")]
    public Color normalHitColor = Color.red;

    [Tooltip("Tint color used for critical hits.")]
    public Color criticalHitColor = Color.yellow;

    [Tooltip("Temporary scale multiplier used for normal hits.")]
    public float normalHitScaleMultiplier = 1.1f;

    [Tooltip("Temporary scale multiplier used for critical hits.")]
    public float criticalHitScaleMultiplier = 1.2f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float nextDamageTime;
    private bool isDead;
    private Vector3 originalScale;
    private Coroutine hitFeedbackRoutine;

    // Cache all materials so we can flash their colors and then restore them.
    private readonly List<Material> cachedMaterials = new List<Material>();
    private readonly List<Color> originalColors = new List<Color>();

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        originalScale = transform.localScale;
        CacheRenderMaterials();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("EnemySkeleton: No GameObject with tag 'Player' found in scene.", this);
        }
    }

    private void Update()
    {
        if (isDead || playerTransform == null)
        {
            return;
        }

        agent.speed = moveSpeed;
        agent.SetDestination(playerTransform.position);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            TakeDamage(projectileDamage);
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            TakeDamage(projectileDamage);
            Destroy(other.gameObject);
        }
    }

    private void TryDamagePlayer(GameObject other)
    {
        if (isDead || !other.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    public void TakeDamage(float amount)
    {
        ApplyDamage(amount, false);
    }

    /// <summary>
    /// Optional stronger damage path that triggers critical-hit feedback.
    /// </summary>
    public void TakeCriticalDamage(float amount)
    {
        ApplyDamage(amount, true);
    }

    private void ApplyDamage(float amount, bool isCriticalHit)
    {
        if (amount <= 0f || isDead)
        {
            return;
        }

        currentHealth -= amount;
        PlayHitFeedback(isCriticalHit);

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    private void PlayHitFeedback(bool isCriticalHit)
    {
        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
            RestoreOriginalVisuals();
        }

        Color flashColor = isCriticalHit ? criticalHitColor : normalHitColor;
        float scaleMultiplier = isCriticalHit ? criticalHitScaleMultiplier : normalHitScaleMultiplier;
        hitFeedbackRoutine = StartCoroutine(HitFeedbackRoutine(flashColor, scaleMultiplier));
    }

    private IEnumerator HitFeedbackRoutine(Color flashColor, float scaleMultiplier)
    {
        SetMaterialColors(flashColor);
        transform.localScale = originalScale * scaleMultiplier;

        yield return new WaitForSeconds(hitFeedbackDuration);

        RestoreOriginalVisuals();
        hitFeedbackRoutine = null;
    }

    private void CacheRenderMaterials()
    {
        cachedMaterials.Clear();
        originalColors.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                if (material != null && material.HasProperty("_Color"))
                {
                    cachedMaterials.Add(material);
                    originalColors.Add(material.color);
                }
            }
        }
    }

    private void SetMaterialColors(Color color)
    {
        for (int i = 0; i < cachedMaterials.Count; i++)
        {
            if (cachedMaterials[i] != null)
            {
                cachedMaterials[i].color = color;
            }
        }
    }

    private void RestoreOriginalVisuals()
    {
        transform.localScale = originalScale;

        for (int i = 0; i < cachedMaterials.Count; i++)
        {
            if (cachedMaterials[i] != null)
            {
                cachedMaterials[i].color = originalColors[i];
            }
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
            RestoreOriginalVisuals();
            hitFeedbackRoutine = null;
        }

        // Spawn loot BEFORE destroying this enemy.
        if (lootPrefab != null)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
            Debug.Log("EnemySkeleton: Loot dropped.", this);
        }

        Debug.Log("EnemySkeleton: Enemy died.", this);
        Destroy(gameObject);
    }
}
