using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
    [SerializeField] private InputActionAsset Input;
    [SerializeField] private double Maxspeed; // 이동속도 제한을 위한 변수
    private InputAction MoveAction; // 이동 (매니저에서 Y축 이동에 대한 재정의 필요)
    Rigidbody2D Rigidbody;

    Vector2 MoveDirection;
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        MoveAction = Input.FindActionMap("Player").FindAction("Move");
        MoveAction.performed += Move_perform;
    }
    void FixedUpdate()
    {
        Move(MoveDirection);
    }

    void Move_perform(InputAction.CallbackContext obj) //콜백 호출과 실제 이동 분리
    {
        MoveDirection = obj.ReadValue<Vector2>();
        Debug.Log(MoveDirection.x + " " + MoveDirection.y);
    }
    void Move(Vector2 Direction)
    {
        Rigidbody.linearVelocityX = Direction.x;
    }
}