using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Sahne Giriş Animasyonu")]
    public float drawSwordDuration = 1.2f;
    private bool canMove = false;

    private Rigidbody2D rb;
    private Animator anim;

    private float horizontalInput;
    private bool isFacingRight = true;

    [Header("Saldırı ve Combo Ayarları")]
    public Animator slashAnim;
    private int comboStep = 0;
    public float comboResetTime = 0.6f;
    private float lastAttackTime;

    // --- YENİ EKLENEN: YAKIN DÖVÜŞ (MELEE) AYARLARI ---
    [Header("Yakın Dövüş Ayarları")]
    public Transform attackPoint;      // Kılıcın vurduğu merkez nokta
    public float attackRange = 1f;     // Kılıcın menzili (Çapı)
    public LayerMask enemyLayers;      // Hasar verilecek katman (EnemyLayer)
    // --------------------------------------------------

    [Header("Yerde mi Kontrolü")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    private bool canDoubleJump;

    [Header("Bıçak Fırlatma Yeteneği")]
    public GameObject knifePrefab;
    public float knifeSpeed = 15f;
    public float throwCooldown = 5f;
    public float knifeLifeTime = 2f;
    private float lastThrowTime = -5f;
    private GameObject currentKnife;

    [Header("Dash (Atılma) Ayarları")]
    public float dashSpeed = 40f;
    public float dashDistance = 4f;
    public float dashCooldown = 4f;
    private float lastDashTime = -4f;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        canMove = false;
        StartCoroutine(UnlockMovement());
    }

    IEnumerator UnlockMovement()
    {
        yield return new WaitForSeconds(drawSwordDuration);
        canMove = true;
    }

    void Update()
    {
        if (!canMove || isDashing) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canDoubleJump = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (Time.time >= lastDashTime + dashCooldown)
            {
                StartCoroutine(DashRoutine());
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (canDoubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = false;
                anim.Play("JumpUp", -1, 0f);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentKnife != null)
            {
                StartCoroutine(TeleportSequence());
            }
            else if (Time.time >= lastThrowTime + throwCooldown)
            {
                ExecuteThrowKnife();
            }
        }

        if (Time.time - lastAttackTime > comboResetTime && comboStep > 0)
        {
            ResetCombo();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;
        anim.SetTrigger("Dash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        float calculatedDuration = dashDistance / dashSpeed;

        yield return new WaitForSeconds(calculatedDuration);

        rb.linearVelocity = new Vector2(0f, 0f);
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    IEnumerator TeleportSequence()
    {
        Vector3 targetPosition = currentKnife.transform.position;
        Destroy(currentKnife);

        canMove = false;
        rb.linearVelocity = Vector2.zero;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float xSign = isFacingRight ? 1f : -1f;
        Vector3 normalScale = new Vector3(1f * xSign, 1f, 1f);
        Vector3 shrunkScale = new Vector3(0.3f * xSign, 0.3f, 0.3f);

        float t = 0;
        float duration = 0.05f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(normalScale, shrunkScale, t / duration);
            yield return null;
        }

        transform.position = targetPosition;

        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(shrunkScale, normalScale, t / duration);
            yield return null;
        }

        transform.localScale = normalScale;
        rb.gravityScale = originalGravity;
        canMove = true;
    }

    void ExecuteThrowKnife()
    {
        lastThrowTime = Time.time;
        anim.SetTrigger("ThrowKnife");

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 throwDirection = (mousePos - transform.position).normalized;

        if (mousePos.x > transform.position.x && !isFacingRight) Flip();
        else if (mousePos.x < transform.position.x && isFacingRight) Flip();

        currentKnife = Instantiate(knifePrefab, transform.position, Quaternion.identity);
        Destroy(currentKnife, knifeLifeTime);

        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        currentKnife.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        currentKnife.GetComponent<Rigidbody2D>().linearVelocity = throwDirection * knifeSpeed;
    }

    // --- GÜNCELLENEN KISIM: KILIÇLA HASAR VERME ---
    void Attack()
    {
        lastAttackTime = Time.time;
        comboStep++;

        if (comboStep > 3)
        {
            comboStep = 1;
        }

        slashAnim.SetInteger("comboStep", comboStep);
    }

    public void DealDamage()
    {
        // 1. AttackPoint merkezli bir çember oluştur
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // 2. Çembere girenleri yok et
        foreach (Collider2D enemy in hitEnemies)
        {
            Destroy(enemy.gameObject);
        }
    }
    // ----------------------------------------------

    void ResetCombo()
    {
        comboStep = 0;
        slashAnim.SetInteger("comboStep", 0);
    }

    void FixedUpdate()
    {
        if (!canMove || isDashing)
        {
            if (!canMove && !isDashing)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Kılıcın menzilini kırmızı bir çember olarak gösterir
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}