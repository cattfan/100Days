using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownAnimDriver : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool normalizeDiagonal = true;

    Rigidbody2D rb;
    Animator anim;
    Vector2 move;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void OnMove(InputValue value) 
    {
        move = value.Get<Vector2>();
        if (normalizeDiagonal && move.sqrMagnitude > 1f) move = move.normalized;
        anim.SetFloat("MoveX", move.x);
        anim.SetFloat("MoveY", move.y);
        anim.SetFloat("Speed", move.sqrMagnitude); 
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
    }
}