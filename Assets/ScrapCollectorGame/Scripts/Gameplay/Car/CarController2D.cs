using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController2D : MonoBehaviour
{
    [Header("Input")]
    public PlayerInput playerInput;
    public string moveActionName = "Move";

    [Header("Movement (mượt)")]
    public float maxSpeed = 6f;
    public float accel = 20f;
    public float decel = 25f;
    public float stopThreshold = 0.05f;

    [Header("Animation (BlendTree 2D Freeform Directional)")]
    public Animator animator;
    public float animTurnSpeedDeg = 540f;
    public float animDamp = 0.10f;

    [Header("Alignment Fix (nếu art lệch trục)")]
    public float animAngleOffsetDeg = 0f;
    public bool invertX = false, invertY = false, swapXY = false;

    [Header("Initial Direction")]
    public Vector2 initialDirection = Vector2.right;

    private Rigidbody2D rb;
    private InputAction moveAction;
    private Vector2 input;
    private Vector2 desiredVel;
    private Vector2 animDirSmooth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        rb.freezeRotation = true;
        
        animDirSmooth = initialDirection.normalized;
        
        enabled = false;
    }

    void OnEnable()
    {
        if (playerInput && playerInput.actions != null)
        {
            moveAction = playerInput.actions[moveActionName];
            moveAction?.Enable();
        }
    }
    void OnDisable() { moveAction?.Disable(); }

    public void BeginDrive(PlayerInput inputSrc)
    {
        playerInput = inputSrc;
        moveAction = playerInput.actions[moveActionName];
        moveAction?.Enable();
        enabled = true;
        
        if (animator)
        {
            Vector2 currentAnimDir = GetTransformedDirection(animDirSmooth);
            animator.SetFloat("MoveX", currentAnimDir.x);
            animator.SetFloat("MoveY", currentAnimDir.y);
            animator.SetFloat("Speed", 0f);
        }
    }

    public void EndDrive()
    {
        enabled = false;
        moveAction?.Disable();
        input = Vector2.zero;
        desiredVel = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        if (animator) animator.SetFloat("Speed", 0f);
    }

    private Vector2 GetTransformedDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + animAngleOffsetDeg;
        Vector2 transformedDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        
        if (swapXY) transformedDir = new Vector2(transformedDir.y, transformedDir.x);
        
        if (invertX) transformedDir.x = -transformedDir.x;
        if (invertY) transformedDir.y = -transformedDir.y;
        
        return transformedDir.normalized;
    }

    void Update()
    {
        if (moveAction != null) input = moveAction.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input, 1f);
    }

    void FixedUpdate()
    {
        Vector2 targetVel = input * maxSpeed;

        Vector2 curVel = rb.linearVelocity;
        Vector2 diff = targetVel - curVel;
        float rate = (targetVel.sqrMagnitude > curVel.sqrMagnitude) ? accel : decel;
        Vector2 step = Vector2.ClampMagnitude(diff, rate * Time.fixedDeltaTime);
        desiredVel = curVel + step;

        if (desiredVel.magnitude < stopThreshold) desiredVel = Vector2.zero;
        rb.linearVelocity = desiredVel;

        if (!animator) return;

        float speed = desiredVel.magnitude;
        animator.SetFloat("Speed", speed);

        Vector2 targetDir = speed > 0.0001f ? desiredVel.normalized : animDirSmooth;

        float curAng = Mathf.Atan2(animDirSmooth.y, animDirSmooth.x) * Mathf.Rad2Deg;
        float tgtAng = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        float newAng = Mathf.MoveTowardsAngle(curAng, tgtAng, animTurnSpeedDeg * Time.fixedDeltaTime);

        animDirSmooth = new Vector2(Mathf.Cos(newAng * Mathf.Deg2Rad), Mathf.Sin(newAng * Mathf.Deg2Rad)).normalized;

        Vector2 finalAnimDir = GetTransformedDirection(animDirSmooth);

        animator.SetFloat("MoveX", finalAnimDir.x, animDamp, Time.deltaTime);
        animator.SetFloat("MoveY", finalAnimDir.y, animDamp, Time.deltaTime);
    }
}