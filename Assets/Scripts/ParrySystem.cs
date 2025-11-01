using System.Collections;
using UnityEngine;

public class ParrySystem : MonoBehaviour
{
    [Header("Parry Settings")]
    public float parryWindow = 0.3f; // How long parry is active
    public float parryCooldown = 1f;
    public KeyCode parryKey = KeyCode.Q;
    
    [Header("Weight System")]
    public float playerWeight = 50f; // Base player weight
    
    [Header("Parry Effects")]
    public float freezeDuration = 0.15f;
    public Color parryFlashColor = Color.cyan;
    public float flashDuration = 0.2f;
    
    [Header("Knockback")]
    public float parryKnockbackForce = 15f;
    
    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    
    private bool isParrying = false;
    private bool canParry = true;
    private float lastParryTime = -999f;
    private Color originalColor;
    
    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void Update()
    {
        HandleParryInput();
    }
    
    void HandleParryInput()
    {
        if (Input.GetKeyDown(parryKey) && canParry && Time.time >= lastParryTime + parryCooldown)
        {
            StartCoroutine(PerformParry());
        }
    }
    
    IEnumerator PerformParry()
    {
        isParrying = true;
        canParry = false;
        lastParryTime = Time.time;
        
        // Trigger parry animation
        if (animator != null)
        {
            animator.SetTrigger("parry");
        }
        
        Debug.Log("Parry active!");
        
        // Parry window duration
        yield return new WaitForSeconds(parryWindow);
        
        isParrying = false;
        
        // Cooldown
        yield return new WaitForSeconds(parryCooldown - parryWindow);
        
        canParry = true;
    }
    
    // Called by EnemyAttack when attack would hit player
    public bool TryParry(GameObject attacker, Vector2 attackDirection, out bool playerGetsKnockedBack)
    {
        playerGetsKnockedBack = false;
        
        if (!isParrying)
        {
            return false; // Parry failed
        }
        
        // Get enemy weight
        EnemyWeight enemyWeight = attacker.GetComponent<EnemyWeight>();
        float enemyWeightValue = enemyWeight != null ? enemyWeight.weight : 50f;
        
        // Determine who gets knocked back
        if (enemyWeightValue > playerWeight)
        {
            // Heavy enemy - player gets knocked back
            playerGetsKnockedBack = true;
            StartCoroutine(ParryKnockbackPlayer(attackDirection));
        }
        else
        {
            // Equal or lighter enemy - enemy gets knocked back
            playerGetsKnockedBack = false;
            StartCoroutine(ParryKnockbackEnemy(attacker, -attackDirection));
        }
        
        // Trigger parry success effects
        StartCoroutine(ParrySuccessEffects());
        
        Debug.Log("PARRY SUCCESS! Enemy weight: " + enemyWeightValue + " vs Player weight: " + playerWeight);
        
        return true; // Parry successful
    }
    
    IEnumerator ParrySuccessEffects()
    {
        // Freeze time
        Time.timeScale = 0f;
        
        // Flash color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = parryFlashColor;
        }
        
        // Wait in real time (not affected by timeScale)
        yield return new WaitForSecondsRealtime(freezeDuration);
        
        // Unfreeze
        Time.timeScale = 1f;
        
        // Flash back to normal
        yield return new WaitForSeconds(flashDuration);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    IEnumerator ParryKnockbackPlayer(Vector2 direction)
    {
        if (rb != null)
        {
            // Knockback player
            Vector2 knockback = new Vector2(direction.x, 0.3f).normalized * parryKnockbackForce * 0.5f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
        
        yield return null;
    }
    
    IEnumerator ParryKnockbackEnemy(GameObject enemy, Vector2 direction)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        
        if (enemyRb != null)
        {
            // Knockback enemy harder
            Vector2 knockback = new Vector2(direction.x, 0.2f).normalized * parryKnockbackForce;
            enemyRb.linearVelocity = Vector2.zero;
            enemyRb.AddForce(knockback, ForceMode2D.Impulse);
            
            // Stun enemy briefly
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.enabled = false;
                yield return new WaitForSeconds(0.5f);
                if (enemy != null) // Check if enemy still exists
                {
                    enemyAI.enabled = true;
                }
            }
        }
    }
    
    public bool IsParrying()
    {
        return isParrying;
    }
    
    public bool CanParry()
    {
        return canParry;
    }
}
