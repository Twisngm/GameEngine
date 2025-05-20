using UnityEngine;

public class Goblin : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        state = EnemyState.Patrol;
    }

    public override void Attack()
    {
        state = EnemyState.Attack;
        // 애니메이션 재생
        animator.SetTrigger("Attack");
        // 진짜 데미지 주는 부분 예정

    }

    public override void TakeDamage(float damage) 
    {
        // 피격 데미지 계산 
        base.TakeDamage(damage);
        // 살아있으면 애니메이션 재생
        if (!isDead)
        {
            animator.SetTrigger("Hit");
        }

    }

    public override void Die()
    {
        base.Die();
        // 애니메이션 재생
        animator.SetTrigger("Die");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
