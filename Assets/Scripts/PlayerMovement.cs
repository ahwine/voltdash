using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Gerakan")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    private float moveInput;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Double Jump")]
    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Pengecekan Tanah")]
    public Transform groundCheck;
    public float checkRadius = 0.22f;
    public LayerMask groundLayer;

    [Header("Tembak")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;

    [Header("VFX Double Jump")]
    public GameObject doubleJumpSplashPrefab;
    public Transform jumpEffectPoint;

    [Header("Ammo System")]
    public int maxAmmo = 6;
    private int currentAmmo;
    public float shootDelay = 0.3f;
    public float reloadDelay = 1.5f;
    private bool canShoot = true;
    private bool isReloading = false;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    private Vector3 baseScale;
    private Animator animator;
    private Coroutine autoReloadRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentAmmo = maxAmmo;
        jumpsRemaining = maxJumps;
        baseScale = transform.localScale;

        UpdateAmmoUI();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(moveInput));

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput > 0)
            transform.localScale = new Vector3(Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);

        if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
        {
            bool isDoubleJump = !isGrounded;

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpsRemaining--;

            if (isDoubleJump)
            {
                SpawnDoubleJumpSplash();
            }
        }

        if (Input.GetMouseButtonDown(0) && canShoot && !isReloading)
        {
            if (currentAmmo > 0)
                Shoot();
            else
                StartManualReload();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartManualReload();
        }

        wasGrounded = isGrounded;
    }

    void SpawnDoubleJumpSplash()
    {
        if (doubleJumpSplashPrefab == null)
            return;

        Vector3 spawnPosition;

        if (jumpEffectPoint != null)
            spawnPosition = jumpEffectPoint.position;
        else if (groundCheck != null)
            spawnPosition = groundCheck.position;
        else
            spawnPosition = transform.position;

        GameObject splash = Instantiate(doubleJumpSplashPrefab, spawnPosition, Quaternion.identity);

        Vector3 splashScale = splash.transform.localScale;
        splashScale.x = Mathf.Abs(splashScale.x) * (transform.localScale.x > 0 ? 1 : -1);
        splash.transform.localScale = splashScale;
    }

    void Shoot()
    {
        canShoot = false;

        if (animator != null)
            animator.SetTrigger("Shoot");

        Vector2 shootDirection = GetShootDirection();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();

        if (rbBullet != null)
        {
            rbBullet.velocity = shootDirection * bulletSpeed;

            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        currentAmmo--;
        UpdateAmmoUI();

        Invoke(nameof(ResetShoot), shootDelay);

        if (currentAmmo == 0)
        {
            if (autoReloadRoutine != null)
                StopCoroutine(autoReloadRoutine);

            autoReloadRoutine = StartCoroutine(AutoReloadAfterLastShot());
        }
    }

    System.Collections.IEnumerator AutoReloadAfterLastShot()
    {
        yield return new WaitForSeconds(shootDelay);

        if (!isReloading && currentAmmo == 0)
        {
            yield return StartCoroutine(Reload());
        }

        autoReloadRoutine = null;
    }

    void StartManualReload()
    {
        if (autoReloadRoutine != null)
        {
            StopCoroutine(autoReloadRoutine);
            autoReloadRoutine = null;
        }

        StartCoroutine(Reload());
    }

    Vector2 GetShootDirection()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.A)) h = -1f;

        if (h == 0 && v == 0)
        {
            h = transform.localScale.x > 0 ? 1f : -1f;
            v = 0f;
        }

        return new Vector2(h, v).normalized;
    }

    void ResetShoot()
    {
        if (!isReloading)
            canShoot = true;
    }

    System.Collections.IEnumerator Reload()
    {
        if (isReloading)
            yield break;

        isReloading = true;
        canShoot = false;

        if (animator != null)
        {
            animator.ResetTrigger("Shoot");
            animator.SetTrigger("Reload");
        }

        if (ammoText != null)
            ammoText.text = "RELOADING...";

        yield return new WaitForSeconds(reloadDelay);

        currentAmmo = maxAmmo;
        isReloading = false;
        canShoot = true;

        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.04f);
        }

        if (jumpEffectPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(jumpEffectPoint.position, 0.05f);
        }
    }
}