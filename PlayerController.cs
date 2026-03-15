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
    - maxHealth / currentHealth: Health setup.
    - projectilePrefab: Prefab to instantiate on left click.
    - projectileSpawnPoint: Optional spawn point for projectiles.
    - projectileSpeed: Launch speed for projectile Rigidbody.
    - projectileLifetime: Auto-destroy timer for spawned projectiles.
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

    [Tooltip("Projectile launch speed (used if projectile has a Rigidbody).")]
    public float projectileSpeed = 25f;

    [Tooltip("Seconds before spawned projectile is destroyed.")]
    public float projectileLifetime = 5f;

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
        HandleShooting();
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

    private void HandleShooting()
    {
        if (!Input.GetMouseButtonDown(0) || projectilePrefab == null)
        {
            return;
        }

        Transform spawnTransform = projectileSpawnPoint != null ? projectileSpawnPoint : playerCamera;
        Vector3 spawnPosition = spawnTransform != null ? spawnTransform.position : transform.position + transform.forward;
        Quaternion spawnRotation = spawnTransform != null ? spawnTransform.rotation : transform.rotation;

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

        // If projectile has Rigidbody, fire using physics velocity.
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = spawnRotation * Vector3.forward * projectileSpeed;
        }
        else
        {
            // Fallback: add a Rigidbody at runtime so it still moves forward.
            rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearVelocity = spawnRotation * Vector3.forward * projectileSpeed;
        }

        Destroy(projectile, projectileLifetime);
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
