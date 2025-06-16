using UnityEngine;
using Unity.Behavior;
using System.Collections;

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

    [SerializeField] protected float idleTime;
    [SerializeField] protected float sightRange;
    [SerializeField] protected float attackRange;

    [SerializeField] protected float patrolDistance;  // �� ���� �̵��� �ִ� �Ÿ�
    protected float checkDistance = 0.5f;  // �������� üũ�� Raycast �Ÿ�
    protected float raycastHeight = 1f;  // Raycast�� ��� ����

    [SerializeField] protected bool isMovingRight = true;  // �̵� ����
    [SerializeField] protected float patrolTime;

    [SerializeField] protected float attackCoolDown;

    protected float idleTimer = 0f;
    protected float patrolTimer = 0f;
    protected float attackTimer = 0f;

    protected Animator animator;
    protected Rigidbody2D rigidbody2d;
    protected SpriteRenderer sprite;
    protected BoxCollider2D attackCollider;
    protected bool isDead = false;
    protected bool isAttack = false;

    protected GameObject player;
    // �����ϴ� �⺻ �޼��� �ڽ� Ŭ�������� ����
    public virtual void Attack()
    {
        // ���� ����
        if (attackTimer <= 0 && !isAttack) 
        {
            isAttack = true;

            StopCoroutine(AttackSequence());
            // ���� �ڷ�ƾ ����
            StartCoroutine(AttackSequence());

            // ���� �� ��Ÿ�� ����
            attackTimer = attackCoolDown;
        }
    }

   protected virtual IEnumerator AttackSequence()
    {
        yield return null;
    }

    // �浹 üũ
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.CompareTag("Attack"))
        {
            TakeDamage(FindObjectOfType<Player>().GetComponent<Player>().Atkdmg);
        }
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
        Vector3 direction = (target - transform.position).normalized;  // Ÿ�� ���� ���ϱ�
        transform.position += direction * speed * Time.deltaTime;  // �ӵ��� ���� �̵�

        if (direction.x > 0)  // ���������� �̵�
        {
            sprite.flipX = false;  // ��������Ʈ�� �������� ������
        }
        else if (direction.x < 0)  // �������� �̵�
        {
            sprite.flipX = true;  // ��������Ʈ�� ������ ������
        }
        FlipAttackRange();
    }

    // ���� ���� �ݶ��̴� ����
    private void FlipAttackRange()
    {
        Vector3 attackPos = attackCollider.transform.localPosition;
        attackPos.x = Mathf.Abs(attackPos.x) * (sprite.flipX ? -1 : 1);
        attackCollider.transform.localPosition = attackPos;
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
    // ���ǿ� ���� State �����ϴ� �޼���
    protected virtual void HandleState()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        switch (state)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
            case EnemyState.Patrol:
                HandlePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
            case EnemyState.Die:
                HandleDieState();
                break;
        }
    }

    // Idle ���� ó��
    private void HandleIdleState(float distanceToPlayer)
    {
        idleTimer += Time.deltaTime;

        animator.SetBool("Run", false);

        if (idleTimer >= idleTime)
        {
            isMovingRight = Random.Range(0f, 1f) > 0.5f;
            state = EnemyState.Patrol;  // ���� �ð� �� Patrol ���·� ����
            idleTimer = 0f;
        }

        if (distanceToPlayer <= sightRange)
        {
            state = EnemyState.Chase;  // �÷��̾ �þ� ���� ���� ������ Chase ���·� ����
        }
    }

    // Patrol ���� ó��
    private void HandlePatrolState(float distanceToPlayer)
    {
        // Patrol ���� 
        patrolTimer += Time.deltaTime;

        if (patrolTimer >= patrolTime)
        {
            state = EnemyState.Idle;
            patrolTimer = 0f;
        }

        float moveDistance = isMovingRight ? patrolDistance : -patrolDistance;

        // �̵�
        Move(transform.position + new Vector3(moveDistance, 0, 0));  // ���� �Ÿ���ŭ �̵�

        // �������� üũ
        if (IsAtEdge())
        {
            isMovingRight = !isMovingRight;  // ���������� ������ ���� ����
        }

        if (distanceToPlayer <= sightRange)
        {
            state = EnemyState.Chase;  // �þ� ���� ���� ������ Chase ���·� ����
        }
    }

    // Chase ���� ó��
    private void HandleChaseState(float distanceToPlayer)
    {
        // �÷��̾ �߰��Ҷ� y�� ��ġ�� ���� ��ġ�� ����
        Vector3 target = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        Move(target);

        if (distanceToPlayer <= attackRange)
        {
            state = EnemyState.Attack;  // ���� ������ ������ Attack ���·� ����
        }

        if (distanceToPlayer > sightRange)
        {
            state = EnemyState.Patrol;  // �þ� ������ ����� Patrol ���·� ����
        }
    }

    // Attack ���� ó��
    private void HandleAttackState(float distanceToPlayer)
    {
        // ���� ��Ÿ�� üũ
        if (attackTimer <= 0f)
        {
            Attack();  // ���� ����
        }

        if (distanceToPlayer > attackRange && !isAttack)
        {
            state = EnemyState.Chase;  // ���� ������ ����� �ٽ� Chase ���·� ����
        }
    }

    // ���� ��Ÿ�� ����
    protected virtual void HandleCooldowns()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    // Die ���� ó��
    private void HandleDieState()
    {
        Die();
    }

    // ���� üũ
    bool IsAtEdge()
    {
        // �߹� ��ġ�� Raycast�� ���� �������� üũ
        Vector2 rayStart = new Vector2(transform.position.x, transform.position.y - raycastHeight);  // �߹� ��ġ
        Vector2 direction = isMovingRight ? Vector2.right : Vector2.left;  // �̵� ����

        // ���̸� ���� �Ÿ����� ��Ƽ� �÷����� �ִ��� üũ
        RaycastHit2D hit = Physics2D.Raycast(rayStart, direction, checkDistance);

        // Ray�� �������� ������ ��������
        return hit.collider == null;
    }
}