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

    [SerializeField] protected float patrolDistance;  // 한 번에 이동할 최대 거리
    protected float checkDistance = 0.5f;  // 낭떠러지 체크할 Raycast 거리
    protected float raycastHeight = 1f;  // Raycast를 쏘는 높이

    [SerializeField] protected bool isMovingRight = true;  // 이동 방향
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
    // 공격하는 기본 메서드 자식 클래스에서 구현
    public virtual void Attack()
    {
        // 공격 로직
        if (attackTimer <= 0 && !isAttack) 
        {
            isAttack = true;

            StopCoroutine(AttackSequence());
            // 공격 코루틴 실행
            StartCoroutine(AttackSequence());

            // 공격 후 쿨타임 설정
            attackTimer = attackCoolDown;
        }
    }

   protected virtual IEnumerator AttackSequence()
    {
        yield return null;
    }

    // 충돌 체크
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.CompareTag("Attack"))
        {
            TakeDamage(FindObjectOfType<Player>().GetComponent<Player>().Atkdmg);
        }
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
        Vector3 direction = (target - transform.position).normalized;  // 타겟 방향 구하기
        transform.position += direction * speed * Time.deltaTime;  // 속도에 맞춰 이동

        if (direction.x > 0)  // 오른쪽으로 이동
        {
            sprite.flipX = false;  // 스프라이트가 오른쪽을 보도록
        }
        else if (direction.x < 0)  // 왼쪽으로 이동
        {
            sprite.flipX = true;  // 스프라이트가 왼쪽을 보도록
        }
        FlipAttackRange();
    }

    // 공격 범위 콜라이더 반전
    private void FlipAttackRange()
    {
        Vector3 attackPos = attackCollider.transform.localPosition;
        attackPos.x = Mathf.Abs(attackPos.x) * (sprite.flipX ? -1 : 1);
        attackCollider.transform.localPosition = attackPos;
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
    // 조건에 따라서 State 변경하는 메서드
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

    // Idle 상태 처리
    private void HandleIdleState(float distanceToPlayer)
    {
        idleTimer += Time.deltaTime;

        animator.SetBool("Run", false);

        if (idleTimer >= idleTime)
        {
            isMovingRight = Random.Range(0f, 1f) > 0.5f;
            state = EnemyState.Patrol;  // 일정 시간 후 Patrol 상태로 변경
            idleTimer = 0f;
        }

        if (distanceToPlayer <= sightRange)
        {
            state = EnemyState.Chase;  // 플레이어가 시야 범위 내로 들어오면 Chase 상태로 변경
        }
    }

    // Patrol 상태 처리
    private void HandlePatrolState(float distanceToPlayer)
    {
        // Patrol 로직 
        patrolTimer += Time.deltaTime;

        if (patrolTimer >= patrolTime)
        {
            state = EnemyState.Idle;
            patrolTimer = 0f;
        }

        float moveDistance = isMovingRight ? patrolDistance : -patrolDistance;

        // 이동
        Move(transform.position + new Vector3(moveDistance, 0, 0));  // 일정 거리만큼 이동

        // 낭떠러지 체크
        if (IsAtEdge())
        {
            isMovingRight = !isMovingRight;  // 낭떠러지가 있으면 방향 반전
        }

        if (distanceToPlayer <= sightRange)
        {
            state = EnemyState.Chase;  // 시야 범위 내로 들어오면 Chase 상태로 변경
        }
    }

    // Chase 상태 처리
    private void HandleChaseState(float distanceToPlayer)
    {
        // 플레이어를 추격할때 y축 위치는 현재 위치로 고정
        Vector3 target = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        Move(target);

        if (distanceToPlayer <= attackRange)
        {
            state = EnemyState.Attack;  // 공격 범위에 들어오면 Attack 상태로 변경
        }

        if (distanceToPlayer > sightRange)
        {
            state = EnemyState.Patrol;  // 시야 범위를 벗어나면 Patrol 상태로 변경
        }
    }

    // Attack 상태 처리
    private void HandleAttackState(float distanceToPlayer)
    {
        // 공격 쿨타임 체크
        if (attackTimer <= 0f)
        {
            Attack();  // 공격 실행
        }

        if (distanceToPlayer > attackRange && !isAttack)
        {
            state = EnemyState.Chase;  // 공격 범위를 벗어나면 다시 Chase 상태로 변경
        }
    }

    // 공격 쿨타임 감소
    protected virtual void HandleCooldowns()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    // Die 상태 처리
    private void HandleDieState()
    {
        Die();
    }

    // 발판 체크
    bool IsAtEdge()
    {
        // 발밑 위치로 Raycast를 쏴서 낭떠러지 체크
        Vector2 rayStart = new Vector2(transform.position.x, transform.position.y - raycastHeight);  // 발밑 위치
        Vector2 direction = isMovingRight ? Vector2.right : Vector2.left;  // 이동 방향

        // 레이를 일정 거리까지 쏘아서 플랫폼이 있는지 체크
        RaycastHit2D hit = Physics2D.Raycast(rayStart, direction, checkDistance);

        // Ray가 감지되지 않으면 낭떠러지
        return hit.collider == null;
    }
}