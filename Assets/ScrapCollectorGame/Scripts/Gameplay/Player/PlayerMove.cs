using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class TopDownAnimDriver : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool normalizeDiagonal = true;
    [Range(0f, 0.3f)] public float damp = 0.05f;
    public float moveThreshold = 0.01f;    
    public float epsilon = 1e-3f;           

    Rigidbody2D rb;
    Animator anim;
    PlayerInput pi;
    InputAction moveAction;
    Vector2 move, lastMoveDir = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pi = GetComponent<PlayerInput>();
        moveAction = pi.actions["Move"];   
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        move = moveAction.ReadValue<Vector2>();                       
        if (normalizeDiagonal && move.sqrMagnitude > 1f) move = move.normalized;

        if (Mathf.Abs(move.x) < epsilon) move.x = 0f;                

        bool isMoving = move.sqrMagnitude >= moveThreshold;

        if (isMoving)
        {
            anim.SetFloat("MoveX", move.x, damp, Time.deltaTime);     
            anim.SetFloat("MoveY", move.y, damp, Time.deltaTime);
            lastMoveDir = move;
        }
        else
        {
            anim.SetFloat("MoveX", 0f);                              
            anim.SetFloat("MoveY", 0f);
        }

        anim.SetBool("Move", isMoving);
        anim.SetFloat("Speed", move.sqrMagnitude);
       
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
    }
}