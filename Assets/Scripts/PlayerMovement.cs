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

    [Header("Pengecekan Tanah")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Tembak")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    
    [Header("Ammo System")]
    public int maxAmmo = 6;
    private int currentAmmo;
    public float shootDelay = 0.3f;
    public float reloadDelay = 1.5f;
    private bool canShoot = true;
    private bool isReloading = false;
    
    [Header("UI")]
    public TextMeshProUGUI ammoText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Movement Horizontal
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // Flip Sprite (hanya jika ada input horizontal)
        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Shoot
        if (Input.GetMouseButtonDown(0) && canShoot && !isReloading)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                StartCoroutine(Reload());
            }
        }
        
        // Reload Manual (R)
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        canShoot = false;
        
        // Tentukan arah tembak berdasarkan WASD
        Vector2 shootDirection = GetShootDirection();
        
        // Buat peluru
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        
        if (rbBullet != null)
        {
            rbBullet.velocity = shootDirection * bulletSpeed;
            
            // Rotasi peluru menghadap arah gerak
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        currentAmmo--;
        UpdateAmmoUI();
        
        Debug.Log("Tembak ke arah: " + shootDirection + " | Sisa ammo: " + currentAmmo);
        
        // Delay sebelum bisa tembak lagi
        Invoke("ResetShoot", shootDelay);
        
        // Auto reload jika ammo habis
        if (currentAmmo == 0)
        {
            StartCoroutine(Reload());
        }
    }

    Vector2 GetShootDirection()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // Deteksi input WASD
        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        
        // Jika tidak ada input WASD, gunakan arah hadap Volt
        if (horizontal == 0 && vertical == 0)
        {
            horizontal = transform.localScale.x > 0 ? 1f : -1f;
            vertical = 0f;
        }
        
        // Normalisasi agar diagonal tidak lebih cepat
        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        
        return direction;
    }

    void ResetShoot()
    {
        canShoot = true;
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        canShoot = false;
        
        Debug.Log("RELOADING...");
        if (ammoText != null)
            ammoText.text = "RELOADING...";
        
        yield return new WaitForSeconds(reloadDelay);
        
        currentAmmo = maxAmmo;
        isReloading = false;
        canShoot = true;
        
        UpdateAmmoUI();
        Debug.Log("Reload selesai. Ammo: " + currentAmmo);
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
    }
}