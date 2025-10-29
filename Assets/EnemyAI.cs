using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 8f;
    public float stopDistance = 1.5f; // Stop before reaching player
    
    [Header("Detection")]
    public LayerMask playerLayer;
    public Transform raycastOrigin; // Optional: assign a child transform for raycast origin
    
    [Header("References")]
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyHealth enemyHealth;
    
    private bool playerDetected = false;
    private float distanceToPlayer;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Use this transform as raycast origin if none assigned
        if (raycastOrigin == null)
        {
            raycastOrigin = transform;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        DetectPlayer();
        
        if (playerDetected)
        {
            FollowPlayer();
        }
        else
        {
            // Idle when player not detected
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        
        UpdateAnimation();
    }
    
    void DetectPlayer()
    {
        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if player is in range
        if (distanceToPlayer <= detectionRange)
        {
            // Raycast to check if there's a clear line of sight
            Vector2 direction = (player.position - raycastOrigin.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin.position, direction, detectionRange, playerLayer);
            
            // Debug line to visualize raycast
            Debug.DrawRay(raycastOrigin.position, direction * detectionRange, Color.red);
            
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                playerDetected = true;
            }
            else
            {
                playerDetected = false;
            }
        }
        else
        {
            playerDetected = false;
        }
    }
    
    void FollowPlayer()
    {
        // Only move if not too close
        if (distanceToPlayer > stopDistance)
        {
            // Calculate direction to player
            Vector2 direction = (player.position - transform.position).normalized;
            
            // Move towards player
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            
            if (direction.x > 0)
            {
                spriteRenderer.flipX = false; // Try this instead
            }
            else if (direction.x < 0)
            {
                spriteRenderer.flipX = true; // Try this instead
            }
        }
        else
        {
            // Stop moving when close enough
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
    
    void UpdateAnimation()
    {
        // Set walking animation based on velocity
        bool isWalking = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        animator.SetBool("isWalking", isWalking);
    }
    
    // Visualize detection range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}