using UnityEngine;

/*
    PlayerController.cs

    First-person controller with movement, mouse look, shooting, and melee.
    Shooting uses the camera forward direction so movement does not change projectile aim.
*/

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float lookSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public Transform playerCamera;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 25f;
    public float projectileLifetime = 5f;
    public float fireRate = 0.15f;
    public int projectileDamage = 25;
    public float meleeDamage = 25f;
    public float meleeRange = 3f;

    [Header("Legacy Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float pitch;
    private float nextFireTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        currentHealth = maxHealth;
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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * horizontal + transform.forward * vertical) * moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
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

        transform.Rotate(Vector3.up * mouseX);
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
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

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
            Debug.LogError("PlayerController: No valid aiming camera found.", this);
            return;
        }

        Transform spawnTransform = projectileSpawnPoint != null ? projectileSpawnPoint : playerCamera;
        if (spawnTransform == null)
        {
            Debug.LogError("PlayerController: projectileSpawnPoint and playerCamera are both missing.", this);
            return;
        }

        Vector3 spawnPosition = spawnTransform.position;
        Vector3 shootDirection = aimCamera.transform.forward;
        if (shootDirection == Vector3.zero)
        {
            shootDirection = transform.forward;
        }

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.velocity = shootDirection * projectileSpeed;
        rb.angularVelocity = Vector3.zero;

        ProjectileDamage projectileDamageComponent = projectile.GetComponent<ProjectileDamage>();
        if (projectileDamageComponent == null)
        {
            projectileDamageComponent = projectile.AddComponent<ProjectileDamage>();
        }

        projectileDamageComponent.damage = projectileDamage;

        nextFireTime = Time.time + fireRate;
        Debug.Log("Shot fired", this);
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

        Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, meleeRange))
        {
            EnemySkeleton enemy = hit.collider.GetComponentInParent<EnemySkeleton>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)meleeDamage);
                Debug.Log("Melee hit");
                return;
            }
        }

        Debug.Log("Melee missed");
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f)
        {
            return;
        }

        currentHealth -= damageAmount;
        Debug.Log($"PlayerController: Took {damageAmount} damage. Current health: {currentHealth}", this);

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Debug.Log("Player Dead");
        }
    }
}
