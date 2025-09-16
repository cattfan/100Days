using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Cinemachine 3.x

public class CarInteraction : MonoBehaviour
{
    [Header("Car ID")]
    [SerializeField] private string carId; // Unique identifier for this car

    [Header("UI")]
    [SerializeField] private GameObject messageUI;       // "Bấm E để sử dụng phương tiện"
    [SerializeField] private GameObject lockedMessageUI; // "Xe đang khóa"

    [Header("Car refs")]
    [SerializeField] private GameObject carRoot;
    [SerializeField] private CarController2D carController;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Input maps & actions")]
    [SerializeField] private string onFootMapName = "OnFoot";
    [SerializeField] private string vehicleMapName = "Vehicle";
    [SerializeField] private string interactActionName = "Interact";

    [Header("Camera (Cinemachine 3)")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private Transform playerCameraTarget;

    [Header("Anti-stuck")]
    [SerializeField] private float ignoreCollisionSeconds = 0.25f;
    [SerializeField] private float fallbackExitOffset = 0.8f;

    private GameObject player;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private SpriteRenderer[] playerRenderers;
    private bool isPlayerNear, isInCar, isBusy;
    private bool isUnlocked = false; // 🚧 mặc định khoá

    // Methods for save/load system
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
        // Generate unique ID if not set
        if (string.IsNullOrEmpty(carId))
        {
            carId = gameObject.name + "_" + transform.position.ToString();
        }

        player = GameObject.FindWithTag("Player");
        if (!player) { Debug.LogError("[CarInteraction] Không thấy Player (tag=Player)"); return; }

        playerInput = player.GetComponent<PlayerInput>();
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

    private void RebindInteract()
    {
        var a = playerInput.actions[interactActionName];
        if (a == null)
        {
            Debug.LogError($"[CarInteraction] Missing action '{interactActionName}'");
            return;
        }
        if (interactAction != null) interactAction.performed -= OnInteract;
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
                if (messageUI) messageUI.SetActive(true);   // hiện "Bấm E để vào xe"
                if (lockedMessageUI) lockedMessageUI.SetActive(false);
            }
            else
            {
                if (lockedMessageUI) lockedMessageUI.SetActive(true); // hiện "Xe đang khóa"
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

    // ===== Vào xe =====
    private System.Collections.IEnumerator EnterCarRoutine()
    {
        isBusy = true;
        isInCar = true;

        if (seatPoint) player.transform.position = seatPoint.position;

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

    // ===== Thoát xe =====
    private System.Collections.IEnumerator ExitCarRoutine()
    {
        isBusy = true;

        if (carController != null) carController.EndDrive();

        Vector3 pos = exitPoint ? exitPoint.position
                   : seatPoint ? seatPoint.position + new Vector3(fallbackExitOffset, 0, 0)
                   : (carRoot ? carRoot.transform.position + new Vector3(fallbackExitOffset, 0, 0) : player.transform.position);
        player.transform.position = pos;

        StartCoroutine(IgnoreCollisionBriefly());

        if (vcam && playerCameraTarget) vcam.Follow = playerCameraTarget;

        yield return new WaitForEndOfFrame();

        playerInput.SwitchCurrentActionMap(onFootMapName);
        RebindInteract();

        SetPlayerVisible(true);

        if (isPlayerNear && messageUI) messageUI.SetActive(true);

        isInCar = false;
        isBusy = false;
    }

    private System.Collections.IEnumerator IgnoreCollisionBriefly()
    {
        var playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) yield break;

        var carCols = carRoot ? carRoot.GetComponentsInChildren<Collider2D>() : new Collider2D[0];
        foreach (var c in carCols) if (c) Physics2D.IgnoreCollision(playerCol, c, true);
        yield return new WaitForSeconds(ignoreCollisionSeconds);
        foreach (var c in carCols) if (c) Physics2D.IgnoreCollision(playerCol, c, false);
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
    }
#endif
}
