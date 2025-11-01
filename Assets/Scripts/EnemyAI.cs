using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;

    [Header("References")]
    public Transform raycastOrigin;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyHealth enemyHealth;
    private EnemyAttack enemyAttack;

    private bool playerDetected;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        enemyAttack = GetComponent<EnemyAttack>();

        // Get player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (raycastOrigin == null)
            raycastOrigin = transform;
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.isDead) return;
        if (player == null) return;

        DetectPlayer();

        if (playerDetected)
            FollowPlayer();
        else
            StopMovement();
    }

    void DetectPlayer()
    {
        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // simple detection range check
        if (distance <= detectionRange)
            playerDetected = true;
        else
            playerDetected = false;

        Debug.DrawRay(raycastOrigin.position, direction.normalized * detectionRange, Color.red);
    }

    void FollowPlayer()
    {
        if (enemyAttack == null || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        // ⛔ stop moving when within attack range
        if (distance > enemyAttack.attackRange)
        {
            rb.linearVelocity = direction * moveSpeed;
            animator.SetBool("isWalking", true);
        }
        else
        {
            StopMovement();
        }

        HandleFlip(direction.x);
    }

    void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);
    }

    void HandleFlip(float directionX)
    {
        if (directionX > 0 && !isFacingRight)
        {
            isFacingRight = true;
            spriteRenderer.flipX = false;
        }
        else if (directionX < 0 && isFacingRight)
        {
            isFacingRight = false;
            spriteRenderer.flipX = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (raycastOrigin == null)
            raycastOrigin = transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(raycastOrigin.position, detectionRange);

        // Show attack range if EnemyAttack exists
        EnemyAttack ea = GetComponent<EnemyAttack>();
        if (ea != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, ea.attackRange);
        }
    }
}
