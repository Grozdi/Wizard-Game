using UnityEngine;
using UnityEngine.AI;

/*
    EnemySkeleton.cs

    SETUP IN UNITY EDITOR
    1) Add this script to your Skeleton enemy GameObject.
    2) Add a NavMeshAgent component to the same GameObject.
    3) Add a Collider to the enemy (CapsuleCollider/BoxCollider etc.).
       - If using trigger detection, enable "Is Trigger".
       - If using physics collision detection, keep "Is Trigger" off and ensure one side has a Rigidbody.
    4) Bake a NavMesh for your level (Window > AI > Navigation).
    5) Ensure your Player object has the PlayerController script attached.
    6) (Recommended) Tag your projectile prefab as "Projectile" so this enemy can detect projectile hits.

    INSPECTOR FIELDS
    - moveSpeed: NavMeshAgent movement speed.
    - damage: Damage applied to player when colliding.
    - maxHealth: Enemy max health.
    - currentHealth: Runtime enemy health.

    USAGE
    - Enemy constantly chases the Player transform.
    - On contact with Player, calls PlayerController.TakeDamage(damage).
    - On projectile hit, enemy loses health.
    - Enemy drops loot on death if a LootDropper is attached, then destroys itself.
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

    [Header("Health")]
    [Tooltip("Maximum enemy health.")]
    public float maxHealth = 50f;

    [Tooltip("Current enemy health.")]
    public float currentHealth = 50f;

    [Header("Optional Tuning")]
    [Tooltip("Minimum time between damage ticks while touching the player.")]
    public float damageCooldown = 0.75f;

    [Tooltip("Damage taken when hit by an object tagged 'Projectile'.")]
    public float projectileDamage = 25f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private LootDropper lootDropper;
    private float nextDamageTime;
    private bool isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lootDropper = GetComponent<LootDropper>();
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
            Debug.LogWarning("EnemySkeleton: No GameObject with tag 'Player' found in scene.");
        }
    }

    private void Update()
    {
        if (playerTransform == null)
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
        if (!other.CompareTag("Player"))
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

        // If a LootDropper is attached, attempt a loot drop before destroying this enemy.
        if (lootDropper != null)
        {
            lootDropper.DropLoot();
        }

        Destroy(gameObject);
    }
}
