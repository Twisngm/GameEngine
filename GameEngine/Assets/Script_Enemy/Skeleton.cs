using System.Collections;
using UnityEngine;

public class Skeleton : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        attackCollider = transform.Find("SkeletonAttackRange").GetComponent<BoxCollider2D>();
        attackCollider.enabled = false;
        player = GameObject.FindWithTag("Player");
        state = EnemyState.Patrol;
    }
    public override void Attack()
    {
        // �ִϸ��̼� ���
        animator.SetTrigger("Attack");

        base.Attack();
    }

    protected override IEnumerator AttackSequence()
    {
        yield return new WaitForSeconds(0.5f);
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.2f);
        attackCollider.enabled = false;
        isAttack = false;
    }

    public override void TakeDamage(float damage)
    {
        // ��������� �ִϸ��̼� ���
        if (!isDead)
        {
            animator.SetTrigger("Hit");
        }
        // �ǰ� ������ ��� 
        base.TakeDamage(damage);
    }

    public override void Die()
    {
        base.Die();
        // �ִϸ��̼� ���
        // animator.SetTrigger("Die");
    }
    /*IEnumerator DeadDestroy()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }*/
    public override void Move(Vector3 target)
    {
        base.Move(target);
        animator.SetBool("Run", true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            HandleState();
            base.HandleCooldowns();
        }
    }
}
