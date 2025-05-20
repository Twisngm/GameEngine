using System;
using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    // default type ����
    float Atkpos;
    [SerializeField] private InputActionAsset Input;
    //[SerializeField] private float Maxspeed; // �̵��ӵ� ������ ���� ����
    [SerializeField] private float Speed;
    [SerializeField] private float JumpPower; // ������
    [SerializeField] private int Atkdmg; // ���ݷ�
    [SerializeField] float MaxHp; // �÷��̾� �⺻ ü��
    float Hp;
    [SerializeField] float RangeDmg;
    private bool Dead;

    [SerializeField] float Hitforce; // �ǰݽ� ���ư��� ��

    [SerializeField] private GameObject AttackRange;
    private Collider2D AttackCollider;
    private Transform RangeTransform;

    public int CharacterValue; // ���ӸŴ������� Ȯ��&�����ϱ� ���� public

    private InputAction MoveAction; // �̵� (�Ŵ������� Y�� �̵��� ���� ������ �ʿ�)
    private InputAction AttackAction; // �⺻����
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

        PlayerAnimator.SetBool("Jump", false); // ���۽� �������°� �ƴϹǷ� false�� �ʱ�ȭ

        MoveAction = Input.FindActionMap("Player").FindAction("Move"); //�̵��Լ� �ݹ�
        MoveAction.performed += Move_perform;

        AttackAction = Input.FindActionMap("Player").FindAction("Attack"); // �����Լ� �ݹ�
        AttackAction.performed += DefaultAttack_perform;

        Skill1 = Input.FindActionMap("Player").FindAction("Skill1"); // ��ų1
        Skill2 = Input.FindActionMap("Player").FindAction("Skill2"); // ��ų2

        if (CharacterValue == 1) // ĳ���� �߰� ������ ���
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

    void Move_perform(InputAction.CallbackContext obj) //�ݹ� ȣ��� ���� �̵� �и�
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