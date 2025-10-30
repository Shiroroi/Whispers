using UnityEngine;

public class AttackEventRelay : MonoBehaviour
{
    private EnemyAttack enemyAttack;

    void Start()
    {
        enemyAttack = GetComponentInParent<EnemyAttack>();
    }

    public void OnAttacking()
    {
        if (enemyAttack != null)
            enemyAttack.PerformAttackHit();
    }
}