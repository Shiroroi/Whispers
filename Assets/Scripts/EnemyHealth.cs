using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;
    [HideInInspector] public bool isDead = false; // ✅ added

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

        if (bloodSpawnPoint == null)
            bloodSpawnPoint = transform;
    }

    public void TakeDamage(float damage, Vector2 hitDirection)
    {
        if (isDead) return; // ✅ Ignore further damage after death

        currentHealth -= damage;

        if (!isFlashing) StartCoroutine(DamageFlash());
        if (!isShaking) StartCoroutine(ShakeEffect());

        SpawnBloodEffect();

        if (currentHealth <= 0)
        {
            isDead = true; // ✅ mark as dead

            // Apply stronger horizontal knockback on death
            if (rb != null)
            {
                Vector2 horizontalKnockback = new Vector2(hitDirection.x, 0).normalized * knockbackForce * deathKnockbackMultiplier;
                rb.AddForce(horizontalKnockback, ForceMode2D.Impulse);
            }

            StartCoroutine(DelayedDeath());
        }
    }

    IEnumerator DamageFlash()
    {
        isFlashing = true;

        if (flashMaterial != null && spriteRenderer != null)
            spriteRenderer.material = flashMaterial;
        else
            spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(flashDuration);

        if (originalMaterial != null && spriteRenderer != null)
            spriteRenderer.material = originalMaterial;
        else
            spriteRenderer.color = Color.white;

        isFlashing = false;
    }

    void SpawnBloodEffect()
    {
        if (bloodEffect != null)
        {
            GameObject blood = Instantiate(bloodEffect, bloodSpawnPoint.position, Quaternion.identity);
            Destroy(blood, 2f);
        }
    }

    void Die()
    {
        if (isDead)
        {
            Debug.Log(gameObject.name + " died!");
            SpawnBloodEffect();
        }

        Destroy(gameObject);
    }

    IEnumerator ShakeEffect()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            spriteRenderer.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.transform.localPosition = originalPosition;
        isShaking = false;
    }

    IEnumerator DelayedDeath()
    {
        // Disable behavior scripts so enemy fully stops
        EnemyAI ai = GetComponent<EnemyAI>();
        EnemyAttack attack = GetComponent<EnemyAttack>();

        if (ai != null) ai.enabled = false;
        if (attack != null) attack.enabled = false;

        yield return new WaitForSeconds(deathDelay);

        Die();
    }
}
