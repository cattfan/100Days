using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CarInteraction : MonoBehaviour
{
    [Header("Car ID")]
    [SerializeField] private string carId;

    [Header("UI")]
    [SerializeField] private GameObject messageUI;
    [SerializeField] private GameObject lockedMessageUI;

    [Header("Car refs")]
    [SerializeField] private GameObject carRoot;
    [SerializeField] private CarController2D carController;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform exitPoint;

    // <<< THAY ĐỔI 1: Tách biệt action vào xe và ra khỏi xe
    [Header("Input maps & actions")]
    [SerializeField] private string onFootMapName = "OnFoot";
    [SerializeField] private string vehicleMapName = "Vehicle";
    [SerializeField] private string interactActionName = "Interact"; // Dùng để RA KHỎI xe
    [SerializeField] private string enterVehicleActionName = "EnterVehicle"; // Dùng để VÀO xe

    [Header("Camera (Cinemachine 3)")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private Transform playerCameraTarget;

    [Header("Anti-stuck")]
    [SerializeField] private float ignoreCollisionSeconds = 0.5f;
    [SerializeField] private float fallbackExitOffset = 0.8f;

    [Header("Smart Exit System V2")]
    [SerializeField] private float playerRadius = 0.25f;
    [SerializeField] private float exitSearchRadius = 2.5f;
    [SerializeField] private int exitSearchAttempts = 20;
    [SerializeField] private float minDistanceFromCar = 0.8f;
    [SerializeField] private bool useAllLayersCheck = true;
    [SerializeField] private LayerMask specificObstacleLayers = -1;
    [SerializeField] private bool debugExitSystem = true;

    [Header("Advanced Safety")]
    [SerializeField] private string[] blockedTags = { "Wall", "Tilemap" };
    [SerializeField] private bool useSimpleCheck = true;
    [SerializeField] private float emergencyTeleportHeight = 1.5f;

    private GameObject player;
    private PlayerInput playerInput;
    private Animator playerAnimator;
    private InputAction interactAction;
    private SpriteRenderer[] playerRenderers;
    private bool isPlayerNear, isInCar, isBusy;
    private bool isUnlocked = false;

    private Vector3 lastAttemptedExitPos;
    private Vector3 lastSuccessfulExitPos;
    private string lastFailReason = "";

    public string GetCarId() => carId;
    public bool IsUnlocked() => isUnlocked;

    public void UnlockCar()
    {
        isUnlocked = true;
        Debug.Log("[CarInteraction] Xe đã được mở khóa!");
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        Debug.Log($"[CarInteraction] Car {carId} unlock state set to: {unlocked}");
    }

    public CarData GetCarData()
    {
        return new CarData(carId, isUnlocked, transform.position, transform.rotation);
    }

    public void LoadCarData(CarData data)
    {
        if (data != null && data.carId == carId)
        {
            isUnlocked = data.isUnlocked;
            transform.position = data.carPosition;
            transform.rotation = data.carRotation;
            Debug.Log($"[CarInteraction] Loaded car data for {carId}: unlocked={isUnlocked}");
        }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(carId))
        {
            carId = gameObject.name + "_" + transform.position.ToString();
        }

        player = GameObject.FindWithTag("Player");
        if (!player) { Debug.LogError("[CarInteraction] Không thấy Player (tag=Player)"); return; }

        playerInput = player.GetComponent<PlayerInput>();
        playerAnimator = player.GetComponent<Animator>();
        if (playerInput == null || playerInput.actions == null)
        { Debug.LogError("[CarInteraction] PlayerInput/actions NULL"); return; }

        if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != onFootMapName)
            playerInput.SwitchCurrentActionMap(onFootMapName);
        RebindInteract();

        if (!carController && carRoot) carController = carRoot.GetComponent<CarController2D>();

        if (messageUI) messageUI.SetActive(false);
        if (lockedMessageUI) lockedMessageUI.SetActive(false);
        playerRenderers = player.GetComponentsInChildren<SpriteRenderer>(true);
        if (!playerCameraTarget && vcam) playerCameraTarget = vcam.Follow;
    }

    private void OnEnable()
    {
        if (interactAction != null) { interactAction.Enable(); interactAction.performed += OnInteract; }
    }
    private void OnDisable()
    {
        if (interactAction != null) { interactAction.performed -= OnInteract; interactAction.Disable(); }
    }

    // <<< THAY ĐỔI 2: Cập nhật lại toàn bộ hàm này
    private void RebindInteract()
    {
        // Hủy đăng ký sự kiện cũ để tránh bị gọi nhiều lần
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
        }

        string actionToFind = "";

        // Kiểm tra xem người chơi đang ở map nào
        if (playerInput.currentActionMap.name == onFootMapName)
        {
            // Nếu đang đi bộ, tìm Action để VÀO XE
            actionToFind = enterVehicleActionName;
        }
        else if (playerInput.currentActionMap.name == vehicleMapName)
        {
            // Nếu đang trong xe, tìm Action để RA KHỎI XE
            actionToFind = interactActionName;
        }

        if (string.IsNullOrEmpty(actionToFind)) return;

        var a = playerInput.actions[actionToFind];
        if (a == null)
        {
            Debug.LogError($"[CarInteraction] Missing action '{actionToFind}'");
            return;
        }

        interactAction = a;
        interactAction.Enable();
        interactAction.performed += OnInteract;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = true;

        if (!isInCar)
        {
            if (isUnlocked)
            {
                if (messageUI) messageUI.SetActive(true);
                if (lockedMessageUI) lockedMessageUI.SetActive(false);
            }
            else
            {
                if (lockedMessageUI) lockedMessageUI.SetActive(true);
                if (messageUI) messageUI.SetActive(false);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = false;

        if (messageUI) messageUI.SetActive(false);
        if (lockedMessageUI) lockedMessageUI.SetActive(false);
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        if (isBusy) return;

        if (isInCar) StartCoroutine(ExitCarRoutine());
        else if (isPlayerNear)
        {
            if (!isUnlocked)
            {
                if (lockedMessageUI)
                {
                    lockedMessageUI.SetActive(true);
                    CancelInvoke(nameof(HideLockedMsg));
                    Invoke(nameof(HideLockedMsg), 2f);
                }
                Debug.Log("[CarInteraction] Xe đang khóa!");
                return;
            }

            StartCoroutine(EnterCarRoutine());
        }
    }

    private void HideLockedMsg()
    {
        if (lockedMessageUI) lockedMessageUI.SetActive(false);
    }

    private System.Collections.IEnumerator EnterCarRoutine()
    {
        isBusy = true;
        isInCar = true;

        if (seatPoint) player.transform.position = seatPoint.position;

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("MoveX", 0f);
            playerAnimator.SetFloat("MoveY", 0f);
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.SetBool("Move", false);
            playerAnimator.enabled = false;
        }

        SetPlayerVisible(false);
        if (messageUI) messageUI.SetActive(false);

        yield return new WaitForEndOfFrame();

        playerInput.SwitchCurrentActionMap(vehicleMapName);
        RebindInteract();

        if (!carController && carRoot) carController = carRoot.GetComponent<CarController2D>();
        if (carController != null) carController.BeginDrive(playerInput);

        if (vcam)
        {
            if (playerCameraTarget == null) playerCameraTarget = vcam.Follow;
            vcam.Follow = (carRoot ? carRoot.transform : transform);
        }

        isBusy = false;
    }

    private System.Collections.IEnumerator ExitCarRoutine()
    {
        isBusy = true;

        if (carController != null) carController.EndDrive();

        var playerRb = player.GetComponent<Rigidbody2D>();
        bool wasPlayerKinematic = false;
        if (playerRb != null)
        {
            wasPlayerKinematic = playerRb.isKinematic;
            playerRb.isKinematic = true;
        }

        StartCoroutine(IgnoreCollisionBriefly());
        yield return null;

        Vector3 safeExitPos = FindSafeExitPositionV2();
        player.transform.position = safeExitPos;

        Debug.Log($"[CarInteraction] Exit position found: {safeExitPos}, Reason: {lastFailReason}");

        yield return null;

        if (vcam && playerCameraTarget) vcam.Follow = playerCameraTarget;

        yield return new WaitForEndOfFrame();

        playerInput.SwitchCurrentActionMap(onFootMapName);
        RebindInteract();

        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.SetFloat("MoveX", 0f);
            playerAnimator.SetFloat("MoveY", 0f);
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.SetBool("Move", false);
        }

        SetPlayerVisible(true);

        yield return new WaitForSeconds(0.1f);
        if (playerRb != null)
        {
            playerRb.isKinematic = wasPlayerKinematic;
        }

        if (isPlayerNear && messageUI) messageUI.SetActive(true);

        isInCar = false;
        isBusy = false;
    }

    private Vector3 FindSafeExitPositionV2()
    {
        Vector3 carCenter = carRoot ? carRoot.transform.position : transform.position;
        lastFailReason = "";

        if (exitPoint != null)
        {
            if (IsPositionSafeV2(exitPoint.position, "exitPoint"))
            {
                lastSuccessfulExitPos = exitPoint.position;
                lastFailReason = "Using exitPoint";
                return exitPoint.position;
            }
        }

        float[] distances = { minDistanceFromCar, minDistanceFromCar * 1.5f, exitSearchRadius };

        foreach (float distance in distances)
        {
            for (int i = 0; i < exitSearchAttempts; i++)
            {
                float angle = (360f / exitSearchAttempts) * i;
                float rad = angle * Mathf.Deg2Rad;

                Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                Vector3 testPos = carCenter + direction * distance;

                lastAttemptedExitPos = testPos;

                if (IsPositionSafeV2(testPos, $"radial_dist{distance:F1}_angle{angle:F0}"))
                {
                    lastSuccessfulExitPos = testPos;
                    lastFailReason = $"Found at distance {distance:F1}, angle {angle:F0}°";
                    return testPos;
                }
            }
        }

        Vector3[] cardinalDirections = { Vector3.right, Vector3.left, Vector3.up, Vector3.down };

        for (float dist = 0.5f; dist <= exitSearchRadius * 1.5f; dist += 0.3f)
        {
            foreach (var direction in cardinalDirections)
            {
                Vector3 testPos = carCenter + direction * dist;
                if (IsPositionSafeV2(testPos, $"cardinal_{direction}_dist{dist:F1}"))
                {
                    lastSuccessfulExitPos = testPos;
                    lastFailReason = $"Cardinal direction {direction} at distance {dist:F1}";
                    return testPos;
                }
            }
        }

        Vector3[] emergencyPositions = {
            carCenter + Vector3.up * emergencyTeleportHeight,
            carCenter + Vector3.up * emergencyTeleportHeight + Vector3.right * 0.5f,
            carCenter + Vector3.up * emergencyTeleportHeight + Vector3.left * 0.5f,
        };

        foreach (var emergencyPos in emergencyPositions)
        {
            if (IsPositionSafeV2(emergencyPos, "emergency"))
            {
                lastSuccessfulExitPos = emergencyPos;
                lastFailReason = $"Emergency position: {emergencyPos}";
                Debug.LogWarning($"[CarInteraction] Using emergency exit: {emergencyPos}");
                return emergencyPos;
            }
        }

        Vector3 ultimatePos = carCenter + Vector3.right * 0.6f;
        lastSuccessfulExitPos = ultimatePos;
        lastFailReason = "Ultimate fallback - no safe position found";
        Debug.LogError($"[CarInteraction] No safe exit found! Using fallback: {ultimatePos}");
        return ultimatePos;
    }

    private bool IsPositionSafeV2(Vector3 position, string checkReason)
    {
        if (!checkReason.Contains("emergency"))
        {
            Vector3 carCenter = carRoot ? carRoot.transform.position : transform.position;
            float distanceFromCar = Vector3.Distance(position, carCenter);
            if (distanceFromCar < minDistanceFromCar)
            {
                if (debugExitSystem)
                    Debug.Log($"[CarInteraction] Position {position} failed: too close to car ({distanceFromCar:F2} < {minDistanceFromCar})");
                return false;
            }
        }

        if (useSimpleCheck)
        {
            return IsPositionSafeSimple(position, checkReason);
        }
        else
        {
            return IsPositionSafeComplex(position, checkReason);
        }
    }

    private bool IsPositionSafeSimple(Vector3 position, string checkReason)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, playerRadius);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            if (hit.CompareTag("Player")) continue;

            if (carRoot != null && (hit.transform.IsChildOf(carRoot.transform) || hit.transform == carRoot.transform))
            {
                if (debugExitSystem)
                    Debug.Log($"[CarInteraction] Position {position} failed: hit car collider {hit.name}");
                return false;
            }

            foreach (string blockedTag in blockedTags)
            {
                if (hit.CompareTag(blockedTag))
                {
                    if (debugExitSystem)
                        Debug.Log($"[CarInteraction] Position {position} failed: hit blocked tag '{blockedTag}' on {hit.name}");
                    return false;
                }
            }

            if (useAllLayersCheck)
            {
                if (!hit.isTrigger)
                {
                    if (debugExitSystem)
                        Debug.Log($"[CarInteraction] Position {position} failed: hit solid collider {hit.name} on layer {hit.gameObject.layer}");
                    return false;
                }
            }
        }

        if (debugExitSystem)
            Debug.Log($"[CarInteraction] Position {position} SAFE ({checkReason})");
        return true;
    }

    private bool IsPositionSafeComplex(Vector3 position, string checkReason)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, playerRadius, specificObstacleLayers);

        if (hit == null) return true;

        if (carRoot != null && (hit.transform.IsChildOf(carRoot.transform) || hit.transform == carRoot.transform))
            return false;

        if (hit.CompareTag("Player"))
            return true;

        if (debugExitSystem)
            Debug.Log($"[CarInteraction] Position {position} failed: LayerMask hit {hit.name}");
        return false;
    }

    private System.Collections.IEnumerator IgnoreCollisionBriefly()
    {
        var playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) yield break;

        var carCols = carRoot ? carRoot.GetComponentsInChildren<Collider2D>() : new Collider2D[0];

        foreach (var c in carCols)
            if (c) Physics2D.IgnoreCollision(playerCol, c, true);

        yield return new WaitForSeconds(ignoreCollisionSeconds);

        foreach (var c in carCols)
            if (c) Physics2D.IgnoreCollision(playerCol, c, false);
    }

    private void SetPlayerVisible(bool enabled)
    {
        if (playerRenderers == null) return;
        foreach (var r in playerRenderers) if (r) r.enabled = enabled;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (seatPoint) { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(seatPoint.position, 0.15f); }
        if (exitPoint) { Gizmos.color = Color.yellow; Gizmos.DrawWireCube(exitPoint.position, new Vector3(0.25f, 0.25f, 0.25f)); }

        if (debugExitSystem)
        {
            Vector3 carCenter = carRoot ? carRoot.transform.position : transform.position;

            Gizmos.color = Color.red;
            DrawCircle(carCenter, minDistanceFromCar);

            Gizmos.color = Color.blue;
            DrawCircle(carCenter, exitSearchRadius);

            if (Application.isPlaying)
            {
                for (int i = 0; i < exitSearchAttempts; i++)
                {
                    float angle = (360f / exitSearchAttempts) * i;
                    float rad = angle * Mathf.Deg2Rad;
                    Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                    Vector3 testPos = carCenter + direction * exitSearchRadius;

                    Gizmos.color = IsPositionSafeV2(testPos, "gizmo_test") ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(testPos, playerRadius);
                }
            }

            if (lastSuccessfulExitPos != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(lastSuccessfulExitPos, playerRadius);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(carCenter, lastSuccessfulExitPos);
            }
        }
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        int segments = 32;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (360f / segments) * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    private void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }
#endif
}