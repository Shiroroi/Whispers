using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color startColor = new Color(1f, 0.5f, 0f, 0.3f); // Orange transparent
    public Color endColor = new Color(1f, 0f, 0f, 0.8f); // Red opaque
    public float pulseSpeed = 5f;
    public bool rotateIndicator = true;
    public float rotationSpeed = 180f;
    
    private SpriteRenderer spriteRenderer;
    private float startTime;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startTime = Time.time;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = startColor;
        }
    }
    
    void Update()
    {
        if (spriteRenderer != null)
        {
            // Pulse color from start to end
            float t = Mathf.PingPong((Time.time - startTime) * pulseSpeed, 1f);
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
        }
        
        // Rotate indicator
        if (rotateIndicator)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}
