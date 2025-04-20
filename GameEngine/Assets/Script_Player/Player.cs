using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    [SerializeField] private InputActionAsset Input;
    [SerializeField] private float Maxspeed; // 이동속도 제한을 위한 변수
    [SerializeField] private float JumpPower; // 점프력
    [SerializeField] private int Atkdmg; // 공격력

    public int CharacterValue; // 게임매니저에서 확인&변경하기 위해 public

    private InputAction MoveAction; // 이동 (매니저에서 Y축 이동에 대한 재정의 필요)
    private InputAction AttackAction; // 기본공격
    private InputAction Skill1;
    private InputAction Skill2;

    Collider2D collision;
    Rigidbody2D Rigidbody;

    [SerializeField] private bool JumpChecker;
    Vector2 MoveDirection;
    void Start()
    {
        JumpChecker = false;
        Rigidbody = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();
        MoveAction = Input.FindActionMap("Player").FindAction("Move");
        MoveAction.performed += Move_perform;

        AttackAction = Input.FindActionMap("Player").FindAction("Attack");
        AttackAction.performed += DefaultAttack_perform;

        Skill1 = Input.FindActionMap("Player").FindAction("Skill1");
        Skill2 = Input.FindActionMap("Player").FindAction("Skill2");

        if (CharacterValue == 1)
        {
            Skill1.performed += SwordSkill1;
            Skill2.performed += SwordSkill2;
        }
    }
    void FixedUpdate()
    {
        MoveDirection = MoveAction.ReadValue<Vector2>();
        Move(MoveDirection);
    }

    void Move_perform(InputAction.CallbackContext obj) //콜백 호출과 실제 이동 분리
    {
        MoveDirection = obj.ReadValue<Vector2>();
        Debug.Log(MoveDirection.x + " " + MoveDirection.y);
    }
    void DefaultAttack_perform(InputAction.CallbackContext obj)
    {
        Debug.Log("DefaultAttack");
    }
    void Move(Vector2 Direction)
    {
        if (Direction.y > 0 && JumpChecker == false)
        {
            Rigidbody.AddForce(new Vector2(0, JumpPower));
            JumpChecker = true;
        }
        Rigidbody.linearVelocityX = Direction.x;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Map")
        {
            JumpChecker = false;
        }
    }
    void SwordSkill1(InputAction.CallbackContext obj)
    {

    }
    void SwordSkill2(InputAction.CallbackContext obj)
    {

    }

    //void Damaged()
}