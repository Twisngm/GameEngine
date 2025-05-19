using UnityEngine;
using Unity.Behavior;

[BlackboardEnum]
public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Die
}

public class Enemy : MonoBehaviour
{
    public EnemyState state;

    [SerializeField] protected float health;
    [SerializeField] protected float damage;
    [SerializeField] protected float speed;

    protected Animator animator;
    protected Rigidbody2D rigidbody2d;
    protected bool isDead = false;
    
    // 공격하는 기본 메서드 자식 클래스에서 구현
    public virtual void Attack()
    {
        Debug.Log("DefaultAttack");
        // 공격 로직
    }

    // 사망하는 기본 메서드
    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 1f);  // 죽은 후 1초 뒤에 삭제
    }

    // 이동하는 기본 메서드
    public virtual void Move(Vector3 target)
    {
        // 타겟으로 이동
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    // 데미지 계산하는 기본 메서드
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }
}