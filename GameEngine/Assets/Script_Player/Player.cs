using System;
using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    // default type 변수
    float Atkpos;
    [SerializeField] private InputActionAsset Input;
    //[SerializeField] private float Maxspeed; // 이동속도 제한을 위한 변수
    [SerializeField] private float Speed;
    [SerializeField] private float JumpPower; // 점프력
    [SerializeField] private int Atkdmg; // 공격력
    [SerializeField] float MaxHp; // 플레이어 기본 체력
    float Hp;
    [SerializeField] float RangeDmg;
    private bool Dead;

    [SerializeField] float Hitforce; // 피격시 날아가는 힘

    [SerializeField] private GameObject AttackRange;
    private Collider2D AttackCollider;
    private Transform RangeTransform;

    public int CharacterValue; // 게임매니저에서 확인&변경하기 위해 public

    private InputAction MoveAction; // 이동 (매니저에서 Y축 이동에 대한 재정의 필요)
    private InputAction AttackAction; // 기본공격
    private InputAction Skill1;
    private InputAction Skill2;

    Collider2D collision;
    Rigidbody2D Rigidbody;
    Animator PlayerAnimator;
    SpriteRenderer sprite;

    Vector2 MoveDirection;

    private GameObject CollisionPlatform;

    PlayerSkill PSkill;
    void Start()
    {
        Dead = false;
        Rigidbody = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();
        PlayerAnimator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        AttackCollider = AttackRange.GetComponent<Collider2D>();
        RangeTransform = AttackRange.GetComponent<Transform>();

        PlayerAnimator.SetBool("Jump", false); // 시작시 점프상태가 아니므로 false로 초기화

        MoveAction = Input.FindActionMap("Player").FindAction("Move"); //이동함수 콜백
        MoveAction.performed += Move_perform;

        AttackAction = Input.FindActionMap("Player").FindAction("Attack"); // 공격함수 콜백
        AttackAction.performed += DefaultAttack_perform;

        Skill1 = Input.FindActionMap("Player").FindAction("Skill1"); // 스킬1
        Skill2 = Input.FindActionMap("Player").FindAction("Skill2"); // 스킬2

        if (CharacterValue == 1) // 캐릭터 추가 구현시 사용
        {
            Skill1.performed += SwordSkill1;
            Skill2.performed += PSkill.Throw;
        }

        AttackCollider.enabled = false;
        Hp = MaxHp;
    }
    void FixedUpdate()
    {
        MoveDirection = MoveAction.ReadValue<Vector2>();
        Move(MoveDirection);
    }

    void Move_perform(InputAction.CallbackContext obj) //콜백 호출과 실제 이동 분리
    {
        MoveDirection = obj.ReadValue<Vector2>();
        PlayerAnimator.SetBool("Walk", true);
        Debug.Log(MoveDirection.x + " " + MoveDirection.y);
    }
    void DefaultAttack_perform(InputAction.CallbackContext obj)
    {
        if (PlayerAnimator.GetBool("Attack") == false && PlayerAnimator.GetBool("Jump") == false)
        {
            Debug.Log("DefaultAttack");

            RangeTransform.localPosition = sprite.flipX == true ? new Vector3(0.25f, 1, 0) : new Vector3(-0.25f, 1, 0);

            PlayerAnimator.SetBool("Attack", true);
            
            StopCoroutine("AttackCoroutine");
            StartCoroutine("AttackCoroutine");
        }
    }

    IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.4f);
        AttackCollider.enabled = true;
        yield return new WaitForSeconds(0.3f);
        AttackCollider.enabled = false;
        PlayerAnimator.SetBool("Attack", false);
        yield break;
    }

    void Move(Vector2 Direction)
    {
        if (PlayerAnimator.GetBool("Attack") == false && !PlayerAnimator.GetBool("Hit"))
        {
            if (Direction.y > 0 && PlayerAnimator.GetBool("Jump") == false && Rigidbody.linearVelocityY == 0)
            {
                Rigidbody.AddForce(new Vector2(0, JumpPower));
                PlayerAnimator.SetBool("Jump", true);
            }
            else if (Direction.y < 0 && CollisionPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }
            Rigidbody.linearVelocityX = Speed * MoveDirection.x;
        }
        
        if (Direction.x == 0) {PlayerAnimator.SetBool("Walk", false); }
        else
        {
            if (Direction.x > 0)
                sprite.flipX = true;
            else
                sprite.flipX = false;
        }
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Map")
        {
            PlayerAnimator.SetBool("Jump", false);
            if (collision.gameObject.layer != 7)
            {
                CollisionPlatform = collision.gameObject;
            }
        }
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Damaged");
            float Dmg = 10;// = collision.gameObject.GetComponent<Enemy>().Dmg
            Damaged(Dmg, collision.transform.position);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Map"))
        {
            CollisionPlatform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = CollisionPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(collision, platformCollider);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(collision, platformCollider, false);
    }


    void SwordSkill1(InputAction.CallbackContext obj)
    {

    }
    void Throw(InputAction.CallbackContext obj)
    {

    }

    void Damaged(float Dmg, Vector2 targetPos)
    {
        Hp -= Dmg;
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        if (Hp > 0)
        {
            PlayerAnimator.SetBool("Hit", true);
            Invoke("OffDamaged", 0.5f);
        }
        else { PlayerAnimator.SetBool("Dead", true); Dead = true; }
        Rigidbody.AddForce(new Vector2(dirc * Hitforce, 1), ForceMode2D.Impulse);
        sprite.color = new Color(1, 1, 1, 0.4f);
    }
    void OffDamaged()
    {
        PlayerAnimator.SetBool("Hit", false);
        sprite.color = new Color(1, 1, 1, 1);
    }

}