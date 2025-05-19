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
        // �ִϸ��̼� ���
        animator.SetTrigger("Attack");
        // ��¥ ������ �ִ� �κ� ����

    }

    public override void TakeDamage(float damage) 
    {
        // �ǰ� ������ ��� 
        base.TakeDamage(damage);
        // ��������� �ִϸ��̼� ���
        if (!isDead)
        {
            animator.SetTrigger("Hit");
        }

    }

    public override void Die()
    {
        base.Die();
        // �ִϸ��̼� ���
        animator.SetTrigger("Die");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
