using UnityEngine;
//using Unity.Behavior;

//[BlakcboardEnum]
public enum EnemyState
{
    Idle,
    Patrol,
    Wander,
    Chase,
    Attack
}

public class Enemy : MonoBehaviour
{
    public EnemyState state;

    [SerializeField] protected float health;
    [SerializeField] protected float damage;
    [SerializeField] protected float speed;

    protected Animator animator;
    protected bool isDead = false;
    
    // �����ϴ� �⺻ �޼��� �ڽ� Ŭ�������� ����
    public virtual void Attack()
    {
        Debug.Log("DefaultAttack");
        // ���� ����
    }

    // ����ϴ� �⺻ �޼���
    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 1f);  // ���� �� 1�� �ڿ� ����
    }

    // �̵��ϴ� �⺻ �޼���
    public virtual void Move(Vector3 target)
    {
        // Ÿ������ �̵�
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    // ������ ����ϴ� �⺻ �޼���
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