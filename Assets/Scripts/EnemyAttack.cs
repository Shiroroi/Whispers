using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;   // Total time between attacks
    public Vector2 attackSize = new Vector2(1f, 1f);
    public Vector2 attackOffset = new Vector2(1f, 0f);
    public LayerMask playerLayer;

    [Header("References")]
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyAI enemyAI;
    private AttackTelegraph attackTelegraph;

    private bool isAttacking;
    private float lastAttackEndTime;

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

        // check if cooldown has finished and player in range
        if (distance <= attackRange && Time.time >= lastAttackEndTime + attackCooldown)
        {
            isAttacking = true;
            StartCoroutine(AttackSequence());
        }
    }

    IEnumerator AttackSequence()
    {
        // stop AI movement
        if (enemyAI != null) enemyAI.enabled = false;

        // show telegraph (always consistent)
        if (attackTelegraph != null)
            yield return attackTelegraph.ShowTelegraph();
        // ensure the telegraph visuals have truly finished
        yield return new WaitUntil(() => !attackTelegraph.IsTelegraphActive);


        // trigger attack animation (actual hit synced via Animation Event)
        if (animator != null)
            animator.SetTrigger("Attack1");

        // wait for animation to finish automatically
        float animLength = GetAnimationLength("Attack1");
        yield return new WaitForSeconds(animLength);

        // resume AI and mark cooldown
        if (enemyAI != null && gameObject != null)
            enemyAI.enabled = true;

        lastAttackEndTime = Time.time;
        isAttacking = false;
    }

    float GetAnimationLength(string stateName)
    {
        if (animator == null) return 0.5f;
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == stateName)
                return clip.length;
        }
        return 0.5f; // fallback
    }

    // Animation event calls this
    public void PerformAttackHit()
    {
        if (spriteRenderer == null) return;

        float direction = spriteRenderer.flipX ? -1f : 1f;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPos, attackSize, 0f, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                ParrySystem parrySystem = hit.GetComponent<ParrySystem>();

                if (parrySystem != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    bool playerGetsKnockedBack;
                    bool parried = parrySystem.TryParry(gameObject, knockbackDir, out playerGetsKnockedBack);
                    if (parried) return; // parry successful, cancel damage
                }

                if (playerHealth != null && !playerHealth.IsInvincible())
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    playerHealth.TakeDamage(attackDamage, knockbackDir);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        float direction = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPos, attackSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
