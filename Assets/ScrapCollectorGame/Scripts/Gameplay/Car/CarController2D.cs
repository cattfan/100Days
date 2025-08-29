using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController2D : MonoBehaviour
{
    [Header("Input")]
    public PlayerInput playerInput;                  // CarInteraction gán khi vào xe
    public string moveActionName = "Move";           // Vector2 (WASD/Stick)

    [Header("Movement (mượt)")]
    public float maxSpeed = 6f;                      // tốc độ tối đa
    public float accel = 20f;                        // gia tốc khi nhấn
    public float decel = 25f;                        // giảm tốc khi thả
    public float stopThreshold = 0.05f;              // dưới ngưỡng coi như đứng yên

    [Header("Animation (BlendTree 2D Freeform Directional)")]
    public Animator animator;                        // cần MoveX, MoveY, Speed
    public float animTurnSpeedDeg = 540f;            // tốc độ quay hướng anim (độ/giây)
    public float animDamp = 0.10f;                   // damping cho SetFloat

    [Header("Alignment Fix (nếu art lệch trục)")]
    public float animAngleOffsetDeg = 0f;            // thử 90 / -90 / 180 nếu thấy lệch
    public bool invertX = false, invertY = false, swapXY = false;

    // --- runtime ---
    private Rigidbody2D rb;
    private InputAction moveAction;
    private Vector2 input;                           // input người chơi
    private Vector2 desiredVel;                      // vận tốc mục tiêu (sau accel/decel)
    private Vector2 animDirSmooth = Vector2.up;      // vector hướng đưa vào Animator (mượt)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        // KHÔNG xoay thân xe (để tránh bẻ sprite)
        rb.freezeRotation = true;
        enabled = false; // bật khi BeginDrive()
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

    void Update()
    {
        if (moveAction != null) input = moveAction.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input, 1f);
    }

    void FixedUpdate()
    {
        // 1) Tính vận tốc mục tiêu (không snap 4 hướng)
        Vector2 targetVel = input * maxSpeed;

        // 2) Gia/giảm tốc mượt
        Vector2 curVel = rb.linearVelocity;
        Vector2 diff = targetVel - curVel;
        float rate = (targetVel.sqrMagnitude > curVel.sqrMagnitude) ? accel : decel;
        Vector2 step = Vector2.ClampMagnitude(diff, rate * Time.fixedDeltaTime);
        desiredVel = curVel + step;

        if (desiredVel.magnitude < stopThreshold) desiredVel = Vector2.zero;
        rb.linearVelocity = desiredVel;

        // 3) Animator
        if (!animator) return;

        float speed = desiredVel.magnitude;
        animator.SetFloat("Speed", speed);

        // ---- QUAY HƯỚNG MƯỢT BẰNG GÓC ----
        // hướng đích theo vận tốc (nếu đứng yên, giữ hướng cũ)
        Vector2 targetDir = speed > 0.0001f ? desiredVel.normalized : animDirSmooth;

        // lấy góc hiện tại & mục tiêu
        float curAng = Mathf.Atan2(animDirSmooth.y, animDirSmooth.x) * Mathf.Rad2Deg;
        float tgtAng = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        // quay mượt theo tốc độ độ/giây
        float newAng = Mathf.MoveTowardsAngle(curAng, tgtAng, animTurnSpeedDeg * Time.fixedDeltaTime);

        // áp offset / đảo trục nếu cần
        newAng += animAngleOffsetDeg;
        Vector2 newDir = new Vector2(Mathf.Cos(newAng * Mathf.Deg2Rad), Mathf.Sin(newAng * Mathf.Deg2Rad));
        if (swapXY) newDir = new Vector2(newDir.y, newDir.x);
        if (invertX) newDir.x = -newDir.x;
        if (invertY) newDir.y = -newDir.y;

        animDirSmooth = newDir.normalized;

        // gửi vào BlendTree
        animator.SetFloat("MoveX", animDirSmooth.x, animDamp, Time.deltaTime);
        animator.SetFloat("MoveY", animDirSmooth.y, animDamp, Time.deltaTime);
    }
}
