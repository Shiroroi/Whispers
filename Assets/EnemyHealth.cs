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
    private Rigidbody2D rb;
    
    private bool isFlashing = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
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
        
        // Spawn blood effect
        SpawnBloodEffect();
        
       
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
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
}
