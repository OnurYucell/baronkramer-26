using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float bulletSpeed = 10f;

    [Header("Yapay Zeka Ayarları")]
    public float aggroRangeX = 12f;
    public float startDelay = 3f;

    // --- YENİ EKLENEN DEVRİYE (PATROL) AYARLARI ---
    [Header("Devriye Ayarları")]
    public float walkSpeed = 3f;      // Düşmanın yürüme hızı
    private int walkDirection = -1;   // Başlangıçta sola yürüsün (-1 = sol, 1 = sağ)
    private Rigidbody2D rb;
    private bool isPlayerInAggro = false; // Oyuncu menzilde mi?
    // ----------------------------------------------

    private float nextFireTime;
    private Transform player;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // Rigidbody'i koda bağladık

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        nextFireTime = Time.time + startDelay;
    }

    void Update()
    {
        if (player == null) return;

        float distanceX = Mathf.Abs(player.position.x - transform.position.x);

        // Oyuncu menzilde mi kontrolü
        if (distanceX <= aggroRangeX)
        {
            // OYUNCU MENZİLDEYSE: Dur ve ateşe başla
            isPlayerInAggro = true;

            // Yürümeyi durdur (Hızı sıfırla)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // Animasyonu bekleme/ateş konumuna al
            anim.SetFloat("Speed", 0f);

            // Düşmanı oyuncuya döndür
            if (player.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (player.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
        else
        {
            // OYUNCU MENZİLDE DEĞİLSE: Devriye atmaya devam et
            isPlayerInAggro = false;
        }
    }

    // --- YENİ EKLENEN DEVRİYE HAREKETİ ---
    void FixedUpdate()
    {
        // Eğer oyuncu menzildeyse FixedUpdate (Yürüme) hiç çalışmasın
        if (isPlayerInAggro) return;

        // Düşmanı mevcut yöne doğru yürüt
        rb.linearVelocity = new Vector2(walkDirection * walkSpeed, rb.linearVelocity.y);

        // Animator'a yürüme parametresini gönder
        anim.SetFloat("Speed", Mathf.Abs(walkDirection * walkSpeed));

        // Yönüne göre görseli döndür
        if (walkDirection == 1)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Sağa yürüyorsa
        }
        else if (walkDirection == -1)
        {
            transform.localScale = new Vector3(1, 1, 1); // Sola yürüyorsa
        }
    }

    // YENİ EKLENEN TETİKLEYİCİ: Görünmez duvara çarptığında yön değiştir
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("PatrolPoint"))
        {
            // Yürüyüş yönünü tersine çevir (-1 ise 1 olur, 1 ise -1 olur)
            walkDirection *= -1;
        }
    }
    // -------------------------------------

    void Shoot()
    {
        anim.SetTrigger("Shoot");

        Vector2 direction = (player.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 leftPoint = transform.position + Vector3.left * aggroRangeX;
        Vector3 rightPoint = transform.position + Vector3.right * aggroRangeX;
        Gizmos.DrawLine(leftPoint, rightPoint);
        Gizmos.DrawWireSphere(leftPoint, 0.2f);
        Gizmos.DrawWireSphere(rightPoint, 0.2f);
    }
}