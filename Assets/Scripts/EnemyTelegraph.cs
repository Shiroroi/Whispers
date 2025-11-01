using System.Collections;
using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    [Header("Telegraph Settings")]
    public float telegraphDuration = 0.5f;
    public TelegraphType telegraphType = TelegraphType.Flash;

    [Header("Flash Settings")]
    public Color flashColor = Color.red;
    public int flashCount = 3;

    [Header("Windup Animation")]
    public string windupAnimationTrigger = "windup";

    [Header("Visual Indicator")]
    public GameObject attackIndicatorPrefab;
    public Vector2 indicatorOffset = new Vector2(0.5f, 0.5f);

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;
    private GameObject currentIndicator;

    public bool IsTelegraphActive { get; private set; }

    public enum TelegraphType
    {
        Flash,
        Windup,
        Indicator,
        Combined
    }

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public IEnumerator ShowTelegraph()
    {
        IsTelegraphActive = true;

        switch (telegraphType)
        {
            case TelegraphType.Flash:
                yield return FlashTelegraph();
                break;

            case TelegraphType.Windup:
                yield return WindupTelegraph();
                break;

            case TelegraphType.Indicator:
                yield return IndicatorTelegraph();
                break;

            case TelegraphType.Combined:
                // Run sequentially instead of in parallel
                yield return FlashTelegraph();
                yield return IndicatorTelegraph();
                yield return WindupTelegraph();
                break;
        }

        IsTelegraphActive = false;
    }

    IEnumerator FlashTelegraph()
    {
        if (spriteRenderer == null) yield break;
        float flashInterval = telegraphDuration / (flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }
    }

    IEnumerator WindupTelegraph()
    {
        if (animator != null)
            animator.SetTrigger(windupAnimationTrigger);

        yield return new WaitForSeconds(telegraphDuration);
    }

    IEnumerator IndicatorTelegraph()
    {
        if (attackIndicatorPrefab == null)
        {
            yield return new WaitForSeconds(telegraphDuration);
            yield break;
        }

        Vector3 indicatorPos = transform.position + (Vector3)indicatorOffset;
        currentIndicator = Instantiate(attackIndicatorPrefab, indicatorPos, Quaternion.identity);
        currentIndicator.transform.SetParent(transform);

        float elapsed = 0f;
        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // optional: fade out for a smoother transition
        SpriteRenderer indSR = currentIndicator.GetComponent<SpriteRenderer>();
        if (indSR != null)
        {
            float fadeTime = 0.15f;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                Color c = indSR.color;
                c.a = Mathf.Lerp(1f, 0f, t / fadeTime);
                indSR.color = c;
                yield return null;
            }
        }

        Destroy(currentIndicator);
    }
}
