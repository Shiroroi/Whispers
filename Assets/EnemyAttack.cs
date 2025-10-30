using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public Vector2 attackSize = new Vector2(1f, 1f);
    public Vector2 attackOffset = new Vector2(1f, 0f);
    public LayerMask playerLayer;

    private float lastAttackTime;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyAI enemyAI;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyAI = GetComponent<EnemyAI>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Check if player is within attack range and cooldown expired
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }

    void Attack()
    {
        // Pick a random attack animation (1–5)
        int attackIndex = Random.Range(1, 6);
        if (animator != null)
            animator.SetTrigger("Attack" + attackIndex);
    }

    // Called by animation event
    public void OnAttacking()
    {
        PerformAttackHit();
    }

    public void PerformAttackHit()
    {
        if (spriteRenderer == null) return;

        // Determine attack direction based on sprite flip
        float direction = spriteRenderer.flipX ? -1f : 1f;

        // Calculate mirrored hitbox position
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);

        // Check collisions in hitbox
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPos, attackSize, 0f, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.IsInvincible())
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    playerHealth.TakeDamage(attackDamage, knockbackDir);
                }
            }
        }
    }

    // Debug visualization in Scene view
    void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        float direction = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPos, attackSize);
    }
}