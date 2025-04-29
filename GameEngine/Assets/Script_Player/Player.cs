using System;
using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    [SerializeField] private InputActionAsset Input;
    [SerializeField] private float Maxspeed; // 이동속도 제한을 위한 변수
    [SerializeField] private float JumpPower; // 점프력
    [SerializeField] private int Atkdmg; // 공격력
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

    // default type 변수
    [SerializeField] float MaxHp; // 플레이어 기본 체력
    float Hp;
    [SerializeField] float AtkDmg; // 기본 공격력

    float Atkpos;
    void Start()
    {
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
            Skill2.performed += SwordSkill2;
        }

        AttackCollider.enabled = false;
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
        if (PlayerAnimator.GetBool("Attack") == false)
        {
            Debug.Log("DefaultAttack");
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
        if (Direction.y > 0 && PlayerAnimator.GetBool("Jump") == false)
        {
            Rigidbody.AddForce(new Vector2(0, JumpPower));
            PlayerAnimator.SetBool("Jump", true);
        }
        if (PlayerAnimator.GetBool("Attack") == false)
            Rigidbody.linearVelocityX = Direction.x;

        if (Direction.x > 0)
        {
            RangeTransform.localPosition = new Vector3(0.25f, 1, 0);
            sprite.flipX = true;
        }
        else if (Direction.x == 0) {PlayerAnimator.SetBool("Walk", false); }
        else
        {
            RangeTransform.localPosition = new Vector3(-0.25f, 1, 0); 
            sprite.flipX = false;
        }
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Map")
        {
            PlayerAnimator.SetBool("Jump", false);
        }
        if (collision.tag == "Enemy")
        {
            float Dmg = 10;// = collision.gameObject.GetComponent<Enemy>().Dmg
            Damaged(Dmg);
        }
    }
    void SwordSkill1(InputAction.CallbackContext obj)
    {

    }
    void SwordSkill2(InputAction.CallbackContext obj)
    {

    }

    void Damaged(float Dmg)
    {
        Hp -= Dmg;
        if (Hp > 0 && PlayerAnimator.GetBool("Attack") == true) PlayerAnimator.SetBool("Hit", true);
        else { PlayerAnimator.SetBool("Dead", true); }
    }
}