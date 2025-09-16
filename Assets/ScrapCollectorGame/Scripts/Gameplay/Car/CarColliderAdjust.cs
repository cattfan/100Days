using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CarColliderAdjust : MonoBehaviour
{
    [Header("References")]
    public Animator animator;       // lấy MoveX, MoveY từ BlendTree
    
    private BoxCollider2D hitbox;   // collider vật lý chính
    private Transform colliderTransform; // Transform riêng cho collider nếu cần

    [Header("Kích thước Collider")]
    public Vector2 sizeHorizontal = new Vector2(4.9f, 2.2f); // khi xe nằm ngang
    public Vector2 sizeVertical = new Vector2(2.2f, 4.9f); // khi xe dựng dọc
    public Vector2 baseColliderSize = new Vector2(4.9f, 2.2f); // kích thước cơ bản của xe (dài x rộng)
    
    [Header("Rotation Settings")]
    public bool enableColliderRotation = true; // Bật/tắt tính năng xoay collider
    public bool useDiscreteRotation = true;    // Chỉ xoay 4 hướng (0°, 90°, 180°, 270°)
    public float rotationSmoothSpeed = 10f;    // Tốc độ xoay mượt của collider (khi useDiscreteRotation = false)
    public float minMovementThreshold = 0.1f;  // Ngưỡng tối thiểu để bắt đầu xoay collider
    
    private float currentColliderAngle = 0f;   // Góc hiện tại của collider
    private float targetDiscreteAngle = 0f;    // Góc mục tiêu cho discrete rotation
    private GameObject colliderObject;         // GameObject riêng chứa collider

    // Enum for 4 cardinal directions
    public enum CardinalDirection
    {
        Right = 0,    // 0°   (1, 0)
        Up = 90,      // 90°  (0, 1)  
        Left = 180,   // 180° (-1, 0)
        Down = 270    // 270° (0, -1)
    }

    void Awake()
    {
        SetupColliderSystem();
    }

    void SetupColliderSystem()
    {
        // Tìm collider vật lý hiện tại
        var colliders = GetComponents<BoxCollider2D>();
        foreach (var c in colliders)
        {
            if (!c.isTrigger) { hitbox = c; break; }
        }

        if (!hitbox)
        {
            Debug.LogError("[CarColliderAdjust] Không tìm thấy BoxCollider2D vật lý (isTrigger=false) trên " + name);
            return;
        }

        if (!animator) animator = GetComponentInChildren<Animator>();

        if (enableColliderRotation)
        {
            // Tạo một GameObject con chứa collider để có thể xoay độc lập
            colliderObject = new GameObject("CarPhysicsCollider");
            colliderObject.transform.SetParent(transform);
            colliderObject.transform.localPosition = Vector3.zero;
            colliderObject.transform.localRotation = Quaternion.identity;
            colliderObject.transform.localScale = Vector3.one;

            // Di chuyển collider sang GameObject con
            var newCollider = colliderObject.AddComponent<BoxCollider2D>();
            newCollider.size = hitbox.size;
            newCollider.offset = hitbox.offset;
            newCollider.isTrigger = hitbox.isTrigger;
            newCollider.sharedMaterial = hitbox.sharedMaterial;

            // Xóa collider cũ
            DestroyImmediate(hitbox);
            hitbox = newCollider;
            colliderTransform = colliderObject.transform;
        }
        
        // Set initial collider size - luôn sử dụng kích thước ngang làm base
        if (hitbox) hitbox.size = sizeHorizontal;
    }

    void Update()
    {
        if (!hitbox || !animator) return;

        if (enableColliderRotation && colliderTransform != null)
        {
            if (useDiscreteRotation)
            {
                UpdateColliderDiscreteRotation();
            }
            else
            {
                UpdateColliderSmoothRotation();
            }
        }
        else
        {
            // Legacy behavior - just change size based on primary direction
            UpdateColliderSizeLegacy();
        }
    }

    private void UpdateColliderDiscreteRotation()
    {
        float mx = animator.GetFloat("MoveX");
        float my = animator.GetFloat("MoveY");
        
        // Chỉ cập nhật rotation khi xe đang di chuyển đủ mạnh
        Vector2 moveVector = new Vector2(mx, my);
        if (moveVector.magnitude > minMovementThreshold)
        {
            // Xác định hướng chính dựa trên MoveX và MoveY
            CardinalDirection targetDirection = GetCardinalDirection(mx, my);
            targetDiscreteAngle = (float)targetDirection;
            
            // Snap trực tiếp đến góc mục tiêu (không smooth)
            currentColliderAngle = targetDiscreteAngle;
            
            // Áp dụng rotation cho collider transform
            colliderTransform.localRotation = Quaternion.Euler(0, 0, currentColliderAngle);
        }
        
        // Luôn giữ kích thước ngang, để rotation tự lo việc xoay
        hitbox.size = sizeHorizontal;
    }

    private void UpdateColliderSmoothRotation()
    {
        float mx = animator.GetFloat("MoveX");
        float my = animator.GetFloat("MoveY");
        
        // Chỉ cập nhật rotation khi xe đang di chuyển đủ mạnh
        Vector2 moveVector = new Vector2(mx, my);
        if (moveVector.magnitude > minMovementThreshold)
        {
            // Tính góc mục tiêu dựa trên hướng di chuyển (smooth rotation)
            float targetAngle = Mathf.Atan2(my, mx) * Mathf.Rad2Deg;
            
            // Xoay mượt collider
            currentColliderAngle = Mathf.LerpAngle(currentColliderAngle, targetAngle, 
                rotationSmoothSpeed * Time.deltaTime);
            
            // Áp dụng rotation cho collider transform
            colliderTransform.localRotation = Quaternion.Euler(0, 0, currentColliderAngle);
        }
        
        // Luôn giữ kích thước ngang cho smooth rotation, để rotation tự lo việc xoay
        hitbox.size = sizeHorizontal;
    }

    private CardinalDirection GetCardinalDirection(float mx, float my)
    {
        // Xác định hướng chính dựa trên giá trị MoveX và MoveY lớn nhất
        float absMx = Mathf.Abs(mx);
        float absMy = Mathf.Abs(my);
        
        if (absMx > absMy)
        {
            // Hướng ngang (trái/phải)
            return mx > 0 ? CardinalDirection.Right : CardinalDirection.Left;
        }
        else
        {
            // Hướng dọc (lên/xuống)
            return my > 0 ? CardinalDirection.Up : CardinalDirection.Down;
        }
    }

    private void UpdateColliderSizeLegacy()
    {
        float mx = animator.GetFloat("MoveX");
        float my = animator.GetFloat("MoveY");

        if (Mathf.Abs(mx) > Mathf.Abs(my))
        {
            // xe ngang → collider rộng
            hitbox.size = sizeHorizontal;
        }
        else
        {
            // xe dọc → collider cao
            hitbox.size = sizeVertical;
        }
    }

    // Public method để có thể gọi từ ngoài nếu cần
    public void SetColliderRotation(bool enable)
    {
        if (enable != enableColliderRotation)
        {
            enableColliderRotation = enable;
            if (Application.isPlaying)
            {
                // Rebuild collider system if needed
                SetupColliderSystem();
            }
        }
    }

    public void SetDiscreteRotation(bool discrete)
    {
        useDiscreteRotation = discrete;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        
        // Vẽ collider hiện tại
        BoxCollider2D col = hitbox;
        if (!col) col = GetComponent<BoxCollider2D>();
        if (!col) return;

        Gizmos.color = Color.red;
        
        if (enableColliderRotation && colliderTransform != null)
        {
            // Vẽ collider đã xoay
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(
                colliderTransform.position, 
                colliderTransform.rotation, 
                colliderTransform.lossyScale
            );
            Gizmos.DrawWireCube(col.offset, col.size);
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            // Vẽ collider không xoay
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
        
        // Vẽ mũi tên chỉ hướng di chuyển
        if (Application.isPlaying && enableColliderRotation && animator)
        {
            float mx = animator.GetFloat("MoveX");
            float my = animator.GetFloat("MoveY");
            
            Vector2 moveVector = new Vector2(mx, my);
            if (moveVector.magnitude > 0.01f)
            {
                Vector3 direction = new Vector3(mx, my, 0).normalized;
                Vector3 start = transform.position;
                Vector3 end = start + direction * 3f;
                
                // Mũi tên màu xanh chỉ hướng di chuyển
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(start, end);
                
                // Đầu mũi tên
                Vector3 arrowHead1 = end - direction * 0.5f + Vector3.Cross(direction, Vector3.forward) * 0.3f;
                Vector3 arrowHead2 = end - direction * 0.5f - Vector3.Cross(direction, Vector3.forward) * 0.3f;
                Gizmos.DrawLine(end, arrowHead1);
                Gizmos.DrawLine(end, arrowHead2);
                
                // Hiển thị hướng discrete nếu đang sử dụng
                if (useDiscreteRotation)
                {
                    CardinalDirection dir = GetCardinalDirection(mx, my);
                    Gizmos.color = Color.yellow;
                    
                    // Vẽ text hiển thị hướng (chỉ trong Scene view)
                    Vector3 textPos = start + Vector3.up * 2f;
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(textPos, $"Direction: {dir} ({(int)dir}°)");
                    #endif
                }
            }
        }
    }
#endif
}
