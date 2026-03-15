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

    LOOT SETUP
    - Drag your loot prefab (e.g., BoneDust pickup) into the "Loot Prefab" field.
    - If lootPrefab is null, no loot is dropped.
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

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float nextDamageTime;
    private bool isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
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
        if (amount <= 0f || isDead)
        {
            return;
        }

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

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
