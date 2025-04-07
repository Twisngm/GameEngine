using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
    Rigidbody2D Rigidbody;
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    void OnMove(Vector2 Direction)
    {
        Rigidbody.AddForce(Direction);
    }
}
