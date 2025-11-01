using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    [Header("Damage Flash")]
    public float flashDuration = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    public Material flashMaterial; // Assign a white material in inspector
    
    [Header("Blood Effect")]
    public GameObject bloodEffect; // Assign blood particle prefab
    public Transform bloodSpawnPoint; // Optional: where blood spawns
    
    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float deathKnockbackMultiplier = 1.5f; // Extra knockback on death
    public float deathDelay = 0.5f; // Time before enemy disappears after death
    private Rigidbody2D rb;
    
    [Header("Screen Shake")]
    public float shakeDuration = 0.2f;
    public float shakeIntensity = 0.1f;
    private Vector3 originalPosition;
    
    private bool isFlashing = false;
    private bool isShaking = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            originalPosition = spriteRenderer.transform.localPosition;
        }
        
        // Use this transform if no blood spawn point assigned
        if (bloodSpawnPoint == null)
        {
            bloodSpawnPoint = transform;
        }
    }
    
    public void TakeDamage(float damage, Vector2 hitDirection)
    {
        currentHealth -= damage;
        
        // Trigger damage flash
        if (!isFlashing)
        {
            StartCoroutine(DamageFlash());
        }
        
        // Trigger shake effect
        if (!isShaking)
        {
            StartCoroutine(ShakeEffect());
        }
        
        // Spawn blood effect
        SpawnBloodEffect();
        
        // Check if dead BEFORE applying knockback
        if (currentHealth <= 0)
        {
            // Apply stronger knockback on death (HORIZONTAL ONLY)
            if (rb != null)
            {
                Vector2 horizontalKnockback = new Vector2(hitDirection.x, 0).normalized * knockbackForce * deathKnockbackMultiplier;
                rb.AddForce(horizontalKnockback, ForceMode2D.Impulse);
            }
            
            // Delay death so knockback can play out
            StartCoroutine(DelayedDeath());
        }
    }
    
    IEnumerator DamageFlash()
    {
        isFlashing = true;
        
        // Change to flash material (white)
        if (flashMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = flashMaterial;
        }
        else
        {
            // Alternative: change color to white if no flash material
            spriteRenderer.color = Color.white;
        }
        
        yield return new WaitForSeconds(flashDuration);
        
        // Return to original
        if (originalMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
        
        isFlashing = false;
    }
    
    void SpawnBloodEffect()
    {
        if (bloodEffect != null)
        {
            GameObject blood = Instantiate(bloodEffect, bloodSpawnPoint.position, Quaternion.identity);
            
            // Destroy blood effect after 2 seconds
            Destroy(blood, 2f);
        }
    }
    
    void Die()
    {
        // Add death animation or effects here
        Debug.Log(gameObject.name + " died!");
        
        // Spawn final blood effect
        SpawnBloodEffect();
        
        // Destroy enemy
        Destroy(gameObject);
    }
    
    IEnumerator ShakeEffect()
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            // Random offset for shake
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            
            spriteRenderer.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Return to original position
        spriteRenderer.transform.localPosition = originalPosition;
        isShaking = false;
    }
    
    IEnumerator DelayedDeath()
    {
        // Disable AI so enemy stops moving towards player
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }
        
        // Wait for knockback to play out
        yield return new WaitForSeconds(deathDelay);
        
        // Now actually die
        Die();
    }
}