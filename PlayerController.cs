using UnityEngine;

/*
    PlayerController.cs

    UNITY EDITOR SETUP INSTRUCTIONS
    1) Create a Player GameObject (for example, a Capsule) and add a CharacterController component.
    2) Attach this PlayerController script to that same Player GameObject.
    3) Make the Main Camera a child of the Player object and position it at eye height (for example Y = 1.6).
    4) Drag the child camera Transform into the "Player Camera" field in the Inspector.
    5) Create a projectile prefab (for example, a small Sphere) and optionally add a Rigidbody to it.
    6) Drag that projectile prefab into the "Projectile Prefab" field in the Inspector.
    7) (Optional but recommended) Create an empty child object at the muzzle location and assign it to "Projectile Spawn Point".
       If left unassigned, the script uses the camera position as the spawn point.

    INSPECTOR FIELDS TO TUNE
    - moveSpeed: WASD movement speed.
    - gravity: Downward gravity acceleration.
    - lookSensitivity: Mouse sensitivity for X/Y look.
    - minPitch / maxPitch: Vertical look clamp limits.
    - projectileSpeed: Launch speed for projectile Rigidbody.
    - projectileLifetime: Auto-destroy timer for spawned projectiles.
    - meleeDamage: Damage dealt on right-click melee hit.
    - meleeRange: Max raycast range for melee attacks.
    - maxHealth / currentHealth: Health setup.
*/

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("WASD movement speed in units/second.")]
    public float moveSpeed = 6f;

    [Tooltip("Downward gravity acceleration.")]
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    [Tooltip("Mouse sensitivity for look controls.")]
    public float lookSensitivity = 2f;

    [Tooltip("Minimum vertical look angle.")]
    public float minPitch = -80f;

    [Tooltip("Maximum vertical look angle.")]
    public float maxPitch = 80f;

    [Tooltip("Assign your child camera Transform here.")]
    public Transform playerCamera;

    [Header("Combat")]
    [Tooltip("Projectile prefab to spawn when left mouse is clicked.")]
    public GameObject projectilePrefab;

    [Tooltip("Optional spawn point. If null, camera position is used.")]
    public Transform projectileSpawnPoint;

    [Tooltip("Projectile launch speed.")]
    public float projectileSpeed = 25f;

    [Tooltip("Seconds before spawned projectile is destroyed.")]
    public float projectileLifetime = 5f;

    [Tooltip("Damage dealt with melee on right click.")]
    public float meleeDamage = 25f;

    [Tooltip("Range of melee raycast attack.")]
    public float meleeRange = 3f;

    [Header("Health")]
    [Tooltip("Maximum health value.")]
    public float maxHealth = 100f;

    [Tooltip("Current health value.")]
    public float currentHealth = 100f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float pitch;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // Start full health by default.
        currentHealth = maxHealth;

        // Lock and hide cursor for FPS controls.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleCombat();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // Movement relative to player's facing direction.
        Vector3 move = (transform.right * horizontal + transform.forward * vertical) * moveSpeed;

        // Apply gravity.
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // small downward force to keep grounded
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (playerCamera == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        // Horizontal look rotates the player body.
        transform.Rotate(Vector3.up * mouseX);

        // Vertical look rotates only the camera.
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleCombat()
    {
        HandleShooting();
        HandleMelee();
    }

    private void HandleShooting()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        Debug.Log("Shoot pressed");

        if (projectilePrefab == null)
        {
            Debug.LogError("PlayerController: projectilePrefab is not assigned.", this);
            return;
        }

        Camera aimCamera = Camera.main;
        if (aimCamera == null && playerCamera != null)
        {
            aimCamera = playerCamera.GetComponent<Camera>();
        }

        if (aimCamera == null)
        {
            Debug.LogError("PlayerController: No valid aiming camera found. Assign playerCamera or tag the camera as MainCamera.", this);
            return;
        }

        Transform spawnTransform = projectileSpawnPoint != null ? projectileSpawnPoint : playerCamera;
        if (spawnTransform == null)
        {
            Debug.LogError("PlayerController: projectileSpawnPoint and playerCamera are both missing.", this);
            return;
        }

        Vector3 spawnPosition = spawnTransform.position;
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = aimCamera.ScreenPointToRay(screenCenter);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100f;
        }

        Vector3 direction = (targetPoint - spawnPosition).normalized;
        if (direction == Vector3.zero)
        {
            direction = spawnTransform.forward;
        }

        Quaternion projectileRotation = Quaternion.LookRotation(direction);
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, projectileRotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.velocity = direction * projectileSpeed;

        Debug.Log("Projectile fired");
        Destroy(projectile, projectileLifetime);
    }

    private void HandleMelee()
    {
        if (!Input.GetMouseButtonDown(1))
        {
            return;
        }

        Camera aimCamera = Camera.main;
        if (aimCamera == null && playerCamera != null)
        {
            aimCamera = playerCamera.GetComponent<Camera>();
        }

        if (aimCamera == null)
        {
            Debug.LogError("PlayerController: No valid aiming camera found for melee.", this);
            Debug.Log("Melee missed");
            return;
        }

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = aimCamera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hit, meleeRange))
        {
            EnemySkeleton enemy = hit.collider.GetComponentInParent<EnemySkeleton>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeDamage);
                Debug.Log("Melee hit");
                return;
            }
        }

        Debug.Log("Melee missed");
    }

    /// <summary>
    /// Call this from enemies/traps/projectiles to damage the player.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f)
        {
            return;
        }

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Debug.Log("Player Dead");
        }
    }
}
