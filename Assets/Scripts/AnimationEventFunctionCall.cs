using UnityEngine;

public class AnimationEventFunctionCall : MonoBehaviour
{
    public float attackMultiplier;
    public Vector2 attackSize = new Vector2(1f, 1f);
    private Vector2 _attackAreaPosition;
    public float offSetX = 1f;
    public float offSetY = 1f;
    public SpriteRenderer spriteRenderer;
    public LayerMask enemyLayer;
    
    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnAttacking(float _attackNum)
    {
        _attackAreaPosition = transform.position;

        offSetX = spriteRenderer.flipX ? -Mathf.Abs(offSetX) : Mathf.Abs(offSetX);

        _attackAreaPosition.x += offSetX;
        _attackAreaPosition.y += offSetY;

        Collider2D[] _hitColliders = Physics2D.OverlapBoxAll(_attackAreaPosition, attackSize, 0f, enemyLayer);
        
        foreach (Collider2D _hitCollider in _hitColliders)
        {
            // Get enemy health component
            EnemyHealth enemyHealth = _hitCollider.gameObject.GetComponent<EnemyHealth>();
            
            if (enemyHealth != null)
            {
                // Calculate damage
                float damage = attackMultiplier * _attackNum;
                
                // Calculate knockback direction (from player to enemy)
                Vector2 knockbackDirection = (_hitCollider.transform.position - transform.position).normalized;
                
                // Deal damage with knockback direction
                enemyHealth.TakeDamage(damage, knockbackDirection);
                
                Debug.Log("Hit " + _hitCollider.gameObject.name + " for " + damage + " damage!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        _attackAreaPosition = transform.position;
        _attackAreaPosition.x += offSetX;
        _attackAreaPosition.y += offSetY;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_attackAreaPosition, attackSize);
    }
}