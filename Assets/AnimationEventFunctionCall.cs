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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get parrent transform, when parent scale.x is change to negative (flip)
        // then can also change my offSetX to negative value
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

            //_hitCollider.gameObject.GetComponent<Health>().TakeDamage(attackMultiplier * _attackNum);
            



            // find the collider that collide with us then use Enemy punya TakeDamage function
            //_hitCollider.GetComponent<Health>().TakeDamage(attackDamage * isAttack);
            // need to assign enemy to enemyLayer
        }


    }

    private void OnDrawGizmos() // Draw the visual of OverlapBoxAll()
    {
        _attackAreaPosition = transform.position;
        _attackAreaPosition.x += offSetX;
        _attackAreaPosition.y += offSetY;



        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_attackAreaPosition, attackSize);
    }
}
