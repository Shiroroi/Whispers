using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public Vector2 attackSize = new Vector2(1f, 1f);
    public Vector2 attackOffset = new Vector2(1f, 0f);
    public LayerMask playerLayer;
    
    [Header("Debug")]
    public bool showDebugLogs = false;

    private float lastAttackTime = -999f;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyAI enemyAI;
    private AttackTelegraph attackTelegraph;
    private bool isAttacking = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyAI = GetComponent<EnemyAI>();
        attackTelegraph = GetComponent<AttackTelegraph>();
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Only check once - when player FIRST enters range after cooldown
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            // Immediately set flag to prevent double-triggering
            isAttacking = true;
            StartCoroutine(AttackSequence());
        }
    }

    IEnumerator AttackSequence()
    {
        // isAttacking already set to true in Update()
        
        // Stop enemy movement during entire attack sequence
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }
        
        // Show telegraph warning - WAIT FOR IT TO COMPLETE
        if (attackTelegraph != null)
        {
            yield return StartCoroutine(attackTelegraph.ShowTelegraph());
        }
        
        // NOW trigger attack animation after telegraph is done
        if (animator != null)
        {
            animator.SetTrigger("Attack1");
        }
        
        // Wait for animation to finish
        yield return new WaitForSeconds(0.5f);
        
        // Set cooldown timer AFTER everything completes
        lastAttackTime = Time.time;
        
        // Resume enemy movement
        if (enemyAI != null && gameObject != null)
        {
            enemyAI.enabled = true;
        }
        
        // Allow attacking again
        isAttacking = false;
    }

    // Called by animation event at the moment of impact
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
                ParrySystem parrySystem = hit.GetComponent<ParrySystem>();
                
                if (parrySystem != null)
                {
                    // Calculate knockback direction (from enemy to player)
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    
                    // Try to parry
                    bool playerGetsKnockedBack;
                    bool parried = parrySystem.TryParry(gameObject, knockbackDir, out playerGetsKnockedBack);
                    
                    if (parried)
                    {
                        Debug.Log("Attack was parried!");
                        return; // Exit - attack was parried
                    }
                }
                
                // If not parried, deal damage normally
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
        
        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}