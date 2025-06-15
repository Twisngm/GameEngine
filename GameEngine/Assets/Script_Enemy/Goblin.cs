using System.Collections;
using Unity.Behavior;
using UnityEngine;

public class Goblin : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        attackCollider = GameObject.Find("GoblinAttakcRange").GetComponent<BoxCollider2D>();
        attackCollider.enabled = false;
        player = GameObject.FindWithTag("Player");
        state = EnemyState.Patrol;
    }

    public override void Attack()
    {
        // 애니메이션 재생
        animator.SetTrigger("Attack");

        base.Attack();
    }

    public override void TakeDamage(float damage) 
    {
        // 살아있으면 애니메이션 재생
        if (!isDead)
        {
            animator.SetTrigger("Hit");
        }
        // 피격 데미지 계산 
        base.TakeDamage(damage);
    }

    public override void Die()
    {
        base.Die();
        // 애니메이션 재생
        animator.SetTrigger("Die");
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
        }
    }
}
