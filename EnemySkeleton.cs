using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
    EnemySkeleton.cs

    Basic NavMesh enemy that chases the player, damages on contact,
    takes projectile/melee damage, flashes on hit, and drops loot on death.
*/

[RequireComponent(typeof(NavMeshAgent))]
public class EnemySkeleton : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;

    [Header("Combat")]
    public float damage = 10f;
    public float damageCooldown = 0.75f;
    public float projectileDamage = 25f;

    [Header("Health")]
    public float maxHealth = 50f;
    public float currentHealth = 50f;

    [Header("Loot")]
    public GameObject lootPrefab;

    [Header("Hit Feedback")]
    public float hitFeedbackDuration = 0.1f;
    public Color normalHitColor = Color.red;
    public Color criticalHitColor = Color.yellow;
    public float normalHitScaleMultiplier = 1.1f;
    public float criticalHitScaleMultiplier = 1.2f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float nextDamageTime;
    private bool isDead;
    private bool hasLoggedMissingPlayer;
    private Vector3 originalScale;
    private Coroutine hitFeedbackRoutine;
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
        TryFindPlayer();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (playerTransform == null)
        {
            TryFindPlayer();
            return;
        }

        Vector3 lookTarget = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        transform.LookAt(lookTarget);

        if (!agent.isOnNavMesh)
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

    private void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            hasLoggedMissingPlayer = false;
            Debug.Log("Player is found", this);
            return;
        }

        if (!hasLoggedMissingPlayer)
        {
            Debug.LogError("EnemySkeleton: Player with tag 'Player' not found.", this);
            hasLoggedMissingPlayer = true;
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

    private void DropLoot()
    {
        if (lootPrefab == null)
        {
            return;
        }

        Debug.Log("Dropping loot", this);
        Instantiate(lootPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
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

        Debug.Log("Enemy died", this);
        DropLoot();
        Destroy(gameObject);
    }
}
