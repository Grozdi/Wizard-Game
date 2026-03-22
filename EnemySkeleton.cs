using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemySkeleton : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;

    [Header("Combat")]
    public int damage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Health")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

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
    private float nextAttackTime;
    private bool isDead;
    private bool hasLoggedMissingPlayer;
    private Vector3 originalScale;
    private Coroutine hitFeedbackRoutine;
    private readonly List<Material> cachedMaterials = new List<Material>();
    private readonly List<Color> originalColors = new List<Color>();

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalScale = transform.localScale;
        CacheRenderMaterials();
    }

    private void Start()
    {
        currentHealth = maxHealth;
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

        if (agent.isOnNavMesh)
        {
            agent.speed = moveSpeed;
            agent.SetDestination(playerTransform.position);
        }

        TryAttackPlayer();
    }

    private void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            hasLoggedMissingPlayer = false;
            return;
        }

        if (!hasLoggedMissingPlayer)
        {
            Debug.LogError("EnemySkeleton: Player with tag 'Player' not found.", this);
            hasLoggedMissingPlayer = true;
        }
    }

    private void TryAttackPlayer()
    {
        if (playerTransform == null || Time.time < nextAttackTime)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > attackRange)
        {
            return;
        }

        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else
        {
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }

        nextAttackTime = Time.time + attackCooldown;
    }

    public void TakeDamage(int amount)
    {
        ApplyDamage(amount, false);
    }

    public void TakeCriticalDamage(int amount)
    {
        ApplyDamage(amount, true);
    }

    private void ApplyDamage(int amount, bool isCriticalHit)
    {
        if (amount <= 0 || isDead)
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log($"Enemy hit for {amount} damage. Current health: {currentHealth}", this);
        PlayHitFeedback(isCriticalHit);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
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
        Instantiate(lootPrefab, transform.position + Vector3.up, Quaternion.identity);
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

        Debug.Log("Enemy death", this);
        DropLoot();
        Destroy(gameObject);
    }
}
