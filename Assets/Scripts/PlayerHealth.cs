using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 2f;
    public float blinkSpeed = 0.1f;
    private bool isInvincible = false;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    [Header("References")]
    public SpriteRenderer spriteRenderer; // Assign manually or auto-find
    private Color originalColor;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(float damage, Vector2 hitDirection)
    {
        // Skip if invincible
        if (isInvincible)
            return;

        currentHealth -= damage;

        // Apply knockback
        if (rb != null)
            StartCoroutine(ApplyKnockback(hitDirection));

        // Start invincibility (includes blinking)
        StartCoroutine(InvincibilityFrames());

        // Check for death
        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator ApplyKnockback(Vector2 hitDirection)
    {
        isKnockedBack = true;

        // normalize direction and remove vertical knockback
        Vector2 force = new Vector2(hitDirection.x, 0.5f).normalized * knockbackForce;

        // cancel current velocity before knockback
        rb.linearVelocity = Vector2.zero;

        // apply knockback impulse
        rb.AddForce(force, ForceMode2D.Impulse);

        // debug to confirm
        Debug.Log("Knockback applied: " + force);

        // wait before regaining control
        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }


    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = (c.a == 1f) ? 0.4f : 1f; // fade instead of disappearing
                spriteRenderer.color = c;
            }

            yield return new WaitForSeconds(blinkSpeed);
            elapsed += blinkSpeed;
        }

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f; // reset to normal
            spriteRenderer.color = c;
        }

        isInvincible = false;
    }


    private void Die()
    {
        Debug.Log("Player died!");
        // Add respawn / game over logic here
        Destroy(gameObject);
    }

    // Called by other scripts
    public bool IsInvincible()
    {
        return isInvincible;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }
}
