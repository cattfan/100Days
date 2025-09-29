using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CarColliderAdjust : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    
    private BoxCollider2D hitbox;
    private Transform colliderTransform;

    [Header("Kích thước Collider")]
    public Vector2 sizeHorizontal = new Vector2(4.9f, 2f);
    public Vector2 sizeVertical = new Vector2(2f, 4.9f);
    public Vector2 baseColliderSize = new Vector2(4.9f, 2f);
    
    [Header("Rotation Settings")]
    public bool enableColliderRotation = true;
    public bool useDiscreteRotation = true;
    public float rotationSmoothSpeed = 10f;
    public float minMovementThreshold = 0.1f;
    
    private float currentColliderAngle = 0f;
    private float targetDiscreteAngle = 0f;
    private GameObject colliderObject;

    public enum CardinalDirection
    {
        Right = 0,
        Up = 90,
        Left = 180,
        Down = 270
    }

    void Awake()
    {
        SetupColliderSystem();
    }

    void SetupColliderSystem()
    {
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
            colliderObject = new GameObject("CarPhysicsCollider");
            colliderObject.transform.SetParent(transform);
            colliderObject.transform.localPosition = Vector3.zero;
            colliderObject.transform.localRotation = Quaternion.identity;
            colliderObject.transform.localScale = Vector3.one;

            var newCollider = colliderObject.AddComponent<BoxCollider2D>();
            newCollider.size = hitbox.size;
            newCollider.offset = hitbox.offset;
            newCollider.isTrigger = hitbox.isTrigger;
            newCollider.sharedMaterial = hitbox.sharedMaterial;

            DestroyImmediate(hitbox);
            hitbox = newCollider;
            colliderTransform = colliderObject.transform;
        }
        
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
            UpdateColliderSizeLegacy();
        }
    }

    private void UpdateColliderDiscreteRotation()
    {
        float mx = animator.GetFloat("MoveX");
        float my = animator.GetFloat("MoveY");
        
        Vector2 moveVector = new Vector2(mx, my);
        if (moveVector.magnitude > minMovementThreshold)
        {
            CardinalDirection targetDirection = GetCardinalDirection(mx, my);
            targetDiscreteAngle = (float)targetDirection;
            
            currentColliderAngle = targetDiscreteAngle;
            
            colliderTransform.localRotation = Quaternion.Euler(0, 0, currentColliderAngle);
        }
        
        hitbox.size = sizeHorizontal;
    }

    private void UpdateColliderSmoothRotation()
    {
        float mx = animator.GetFloat("MoveX");
        float my = animator.GetFloat("MoveY");
        
        Vector2 moveVector = new Vector2(mx, my);
        if (moveVector.magnitude > minMovementThreshold)
        {
            float targetAngle = Mathf.Atan2(my, mx) * Mathf.Rad2Deg;
            
            currentColliderAngle = Mathf.LerpAngle(currentColliderAngle, targetAngle, 
                rotationSmoothSpeed * Time.deltaTime);
            
            colliderTransform.localRotation = Quaternion.Euler(0, 0, currentColliderAngle);
        }
        
        hitbox.size = sizeHorizontal;
    }

    private CardinalDirection GetCardinalDirection(float mx, float my)
    {
        float absMx = Mathf.Abs(mx);
        float absMy = Mathf.Abs(my);
        
        if (absMx > absMy)
        {
            return mx > 0 ? CardinalDirection.Right : CardinalDirection.Left;
        }
        else
        {
            return my > 0 ? CardinalDirection.Up : CardinalDirection.Down;
        }
    }

    private void UpdateColliderSizeLegacy()
    {
        float mx = animator.GetFloat("MoveX");
        float my = animator.GetFloat("MoveY");

        if (Mathf.Abs(mx) > Mathf.Abs(my))
        {
            hitbox.size = sizeHorizontal;
        }
        else
        {
            hitbox.size = sizeVertical;
        }
    }

    public void SetColliderRotation(bool enable)
    {
        if (enable != enableColliderRotation)
        {
            enableColliderRotation = enable;
            if (Application.isPlaying)
            {
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
        
        BoxCollider2D col = hitbox;
        if (!col) col = GetComponent<BoxCollider2D>();
        if (!col) return;

        Gizmos.color = Color.red;
        
        if (enableColliderRotation && colliderTransform != null)
        {
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
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
        
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
                
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(start, end);
                
                Vector3 arrowHead1 = end - direction * 0.5f + Vector3.Cross(direction, Vector3.forward) * 0.3f;
                Vector3 arrowHead2 = end - direction * 0.5f - Vector3.Cross(direction, Vector3.forward) * 0.3f;
                Gizmos.DrawLine(end, arrowHead1);
                Gizmos.DrawLine(end, arrowHead2);
                
                if (useDiscreteRotation)
                {
                    CardinalDirection dir = GetCardinalDirection(mx, my);
                    Gizmos.color = Color.yellow;
                    
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
