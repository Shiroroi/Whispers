using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = -999f;
    
    [Header("Attack Hitbox")]
    public Vector2 attackSize = new Vector2(1.5f, 1f);
    public float attackOffsetX = 0.75f;
    public float attackOffsetY = 0f;
    
    [Header("References")]
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public LayerMask playerLayer;
    
    private bool isAttacking = false;
    
    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check if player is in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }
    
    void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        
        // Trigger attack animation if you have one
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
        
        // Perform attack hitbox check immediately
        // If you want delay (for animation), call this from an Animation Event instead
        PerformAttackHit();
    }
    
    // Call this from Animation Event or directly
    public void PerformAttackHit()
    {
        Vector2 attackPosition = transform.position;
        
        // Adjust offset based on sprite flip
        float adjustedOffsetX = spriteRenderer.flipX ? -Mathf.Abs(attackOffsetX) : Mathf.Abs(attackOffsetX);
        attackPosition.x += adjustedOffsetX;
        attackPosition.y += attackOffsetY;
        
        // Check for player in attack area
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackPosition, attackSize, 0f, playerLayer);
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                
                if (playerHealth != null)
                {
                    // Calculate knockback direction (from enemy to player)
                    Vector2 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    
                    // Deal damage
                    playerHealth.TakeDamage(attackDamage, knockbackDirection);
                    
                    Debug.Log("Enemy hit player for " + attackDamage + " damage!");
                }
            }
        }
        
        isAttacking = false;
    }
    
    // Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        // Draw attack range circle
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw attack hitbox
        if (spriteRenderer != null)
        {
            Vector2 attackPosition = transform.position;
            float adjustedOffsetX = spriteRenderer.flipX ? -Mathf.Abs(attackOffsetX) : Mathf.Abs(attackOffsetX);
            attackPosition.x += adjustedOffsetX;
            attackPosition.y += attackOffsetY;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(attackPosition, attackSize);
        }
    }
}
