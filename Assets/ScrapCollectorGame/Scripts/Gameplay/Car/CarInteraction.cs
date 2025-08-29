using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Cinemachine 3.x

public class CarInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject messageUI;      // "Bấm E để sử dụng phương tiện"

    [Header("Car refs")]
    [SerializeField] private GameObject carRoot;        // Thân xe (có Rigidbody2D + CarController2D)
    [SerializeField] private CarController2D carController;
    [SerializeField] private Transform seatPoint;       // Điểm NGỒI khi vào xe
    [SerializeField] private Transform exitPoint;       // Điểm THOÁT khi ra xe

    [Header("Input maps & actions")]
    [SerializeField] private string onFootMapName = "OnFoot";
    [SerializeField] private string vehicleMapName = "Vehicle";
    [SerializeField] private string interactActionName = "Interact"; // Có trong cả 2 map

    [Header("Camera (Cinemachine 3)")]
    [SerializeField] private CinemachineCamera vcam;    // Kéo object "CinemachineCamera" trong scene
    [SerializeField] private Transform playerCameraTarget; // Follow gốc (Player). Có thể để trống

    [Header("Anti-stuck")]
    [SerializeField] private float ignoreCollisionSeconds = 0.25f; // bỏ va chạm Player↔Xe tạm thời
    [SerializeField] private float fallbackExitOffset = 0.8f;      // nếu chưa gán exitPoint

    // runtime
    private GameObject player;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private SpriteRenderer[] playerRenderers;
    private bool isPlayerNear, isInCar, isBusy; // isBusy = debounce

    private void Awake()
    {
        // Tìm Player & Input
        player = GameObject.FindWithTag("Player");
        if (!player) { Debug.LogError("[CarInteraction] Không thấy Player (tag=Player)"); return; }

        playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null || playerInput.actions == null)
        { Debug.LogError("[CarInteraction] PlayerInput/actions NULL"); return; }

        // Đảm bảo đang ở map đi bộ khi vào scene
        if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != onFootMapName)
            playerInput.SwitchCurrentActionMap(onFootMapName);
        RebindInteract();

        // Car
        if (!carController && carRoot) carController = carRoot.GetComponent<CarController2D>();
        if (!carController) Debug.LogError("[CarInteraction] Thiếu CarController2D trên carRoot");

        // UI, camera, renderer
        if (messageUI) messageUI.SetActive(false);
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
            Debug.LogError($"[CarInteraction] Missing action '{interactActionName}' trong map '{playerInput.currentActionMap?.name}'");
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
        if (!isInCar && messageUI) messageUI.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = false;
        if (!isInCar && messageUI) messageUI.SetActive(false);
    }

    // ===== Fix "phải bấm E 2 lần": dùng coroutine + debounce =====
    private void OnInteract(InputAction.CallbackContext _)
    {
        if (isBusy) return;

        if (isInCar) StartCoroutine(ExitCarRoutine());
        else if (isPlayerNear) StartCoroutine(EnterCarRoutine());
    }

    private System.Collections.IEnumerator EnterCarRoutine()
    {
        isBusy = true;

        // Chốt trạng thái vào xe ngay để không gọi lại trong cùng lần nhấn
        isInCar = true;

        // Đặt player về ghế (thị giác)
        if (seatPoint) player.transform.position = seatPoint.position;

        // Ẩn player, ẩn UI
        SetPlayerVisible(false);
        if (messageUI) messageUI.SetActive(false);

        // Chờ kết thúc frame trước khi switch map + giao input (tránh nuốt lần nhấn)
        yield return new WaitForEndOfFrame();

        // Đổi sang map lái xe, lấy lại action Interact của map mới
        playerInput.SwitchCurrentActionMap(vehicleMapName);
        RebindInteract();

        // Bàn giao input cho xe
        if (!carController && carRoot) carController = carRoot.GetComponent<CarController2D>();
        if (carController != null) carController.BeginDrive(playerInput);

        // Camera follow xe
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

        // Ngừng lái xe
        if (carController != null) carController.EndDrive();

        // Chọn vị trí thoát
        Vector3 pos = exitPoint ? exitPoint.position
                   : seatPoint ? seatPoint.position + new Vector3(fallbackExitOffset, 0, 0)
                   : (carRoot ? carRoot.transform.position + new Vector3(fallbackExitOffset, 0, 0) : player.transform.position);
        player.transform.position = pos;

        // Tạm bỏ va chạm Player↔Xe để tránh kẹt
        StartCoroutine(IgnoreCollisionBriefly());

        // Trả camera về Player
        if (vcam && playerCameraTarget) vcam.Follow = playerCameraTarget;

        // Chờ hết frame rồi mới switch map về đi bộ (tránh nuốt input)
        yield return new WaitForEndOfFrame();

        playerInput.SwitchCurrentActionMap(onFootMapName);
        RebindInteract();

        // Hiện Player lại
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
