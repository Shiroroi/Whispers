using System.Collections;
using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    [Header("Telegraph Settings")]
    public float telegraphDuration = 0.5f; // How long warning shows before attack
    public TelegraphType telegraphType = TelegraphType.Flash;
    
    [Header("Flash Settings")]
    public Color flashColor = Color.red;
    public int flashCount = 3;
    
    [Header("Windup Animation")]
    public string windupAnimationTrigger = "windup";
    
    [Header("Visual Indicator")]
    public GameObject attackIndicatorPrefab; // Optional: sprite/particle that appears
    public Vector2 indicatorOffset = new Vector2(0.5f, 0.5f);
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;
    private GameObject currentIndicator;
    
    public enum TelegraphType
    {
        Flash,          // Sprite flashes red
        Windup,         // Play windup animation
        Indicator,      // Spawn visual indicator
        Combined        // All of the above
    }
    
    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    public IEnumerator ShowTelegraph()
    {
        switch (telegraphType)
        {
            case TelegraphType.Flash:
                yield return StartCoroutine(FlashTelegraph());
                break;
            case TelegraphType.Windup:
                yield return StartCoroutine(WindupTelegraph());
                break;
            case TelegraphType.Indicator:
                yield return StartCoroutine(IndicatorTelegraph());
                break;
            case TelegraphType.Combined:
                // Run all at the same time, wait for longest one
                StartCoroutine(FlashTelegraph());
                StartCoroutine(IndicatorTelegraph());
                yield return StartCoroutine(WindupTelegraph());
                break;
        }
    }
    
    IEnumerator FlashTelegraph()
    {
        if (spriteRenderer == null) yield break;
        
        float flashInterval = telegraphDuration / (flashCount * 2);
        
        for (int i = 0; i < flashCount; i++)
        {
            // Flash to warning color
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            
            // Flash back to normal
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }
    }
    
    IEnumerator WindupTelegraph()
    {
        if (animator != null)
        {
            animator.SetTrigger(windupAnimationTrigger);
        }
        
        yield return new WaitForSeconds(telegraphDuration);
    }
    
    IEnumerator IndicatorTelegraph()
    {
        if (attackIndicatorPrefab != null)
        {
            // Spawn indicator
            Vector3 indicatorPos = transform.position + (Vector3)indicatorOffset;
            currentIndicator = Instantiate(attackIndicatorPrefab, indicatorPos, Quaternion.identity);
            currentIndicator.transform.SetParent(transform);
            
            yield return new WaitForSeconds(telegraphDuration);
            
            // Destroy indicator
            if (currentIndicator != null)
            {
                Destroy(currentIndicator);
            }
        }
        else
        {
            yield return new WaitForSeconds(telegraphDuration);
        }
    }
}