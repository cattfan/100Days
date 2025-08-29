using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ItemUI.cs: Xử lý hành vi kéo và thả của vật phẩm
public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI refs (assign in prefab)")]
    public Image icon;
    public TextMeshProUGUI amountText;

    private ItemData itemData;
    private int amount = 1;

    // Biến giúp theo dõi trạng thái
    public Transform originalParent { get; private set; }
    private RectTransform inventoryPanelRect;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    public bool droppedOnValidSlot = false; // Flag mới: true nếu item được thả vào một Slot hợp lệ
    private AudioManagement audioManager;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();

        // Tìm inventory panel
        var inventoryController = FindObjectOfType<InventoryController>();
        if (inventoryController != null)
            inventoryPanelRect = inventoryController.GetInventoryPanel();

        audioManager = FindObjectOfType<AudioManagement>();
    }

    public void Setup(ItemData data, int qty = 1)
    {
        itemData = data;
        amount = Mathf.Max(1, qty);

        if (icon != null && data != null)
            icon.sprite = data.itemIcon;

        UpdateAmountUI();
    }

    public ItemData GetItemData() => itemData;
    public int Amount => amount;

    public void AddAmount(int v)
    {
        amount += v;
        UpdateAmountUI();
    }

    void UpdateAmountUI()
    {
        if (amountText == null) return;

        if (itemData == null) { amountText.text = ""; return; }

        if (amount > 1)
        {
            amountText.text = amount.ToString();
        }
        else
        {
            // Nếu stackable và chỉ có 1 thì ẩn
            if (itemData.isStackable && amount > 1)
            {
                amountText.text = "";
            }
            else
                amountText.text = ""; // Non-stackable thì luôn hiện 1
        }
    }


    // ---- Drag handlers ----
    // Thay thế 3 hàm này trong ItemUI.cs của bạn:

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Gán lại cờ droppedOnValidSlot mỗi khi bắt đầu kéo
        droppedOnValidSlot = false;
        originalParent = transform.parent;

        // Lưu lại slot gốc trước khi kéo
        var originalSlot = originalParent.GetComponent<Slot>();
        if (originalSlot != null)
            originalSlot.currentItem = null;

        if (parentCanvas != null)
            transform.SetParent(parentCanvas.transform, true);
        else
            transform.SetParent(transform.root, true);

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!droppedOnValidSlot)
        {
            // Kiểm tra có trong inventory panel không
            bool isInsideInventoryPanel = IsInsideInventoryPanel(eventData.position, eventData.pressEventCamera);

            if (isInsideInventoryPanel)
            {
                // TRONG panel -> trả về vị trí cũ
                transform.SetParent(originalParent);
                transform.localPosition = Vector3.zero;

                var originalSlot = originalParent.GetComponent<Slot>();
                if (originalSlot != null)
                    originalSlot.currentItem = gameObject;
            }
            else
            {
                // NGOÀI panel -> destroy item
                var originalSlot = originalParent.GetComponent<Slot>();
                if (originalSlot != null)
                    originalSlot.currentItem = null; // Clear slot trước khi destroy

                Destroy(gameObject);
                audioManager?.PlaySFX(audioManager.DropItem);
            }
        }
        else
        {
            // Thả vào slot hợp lệ
            audioManager?.PlaySFX(audioManager.PlaceItem);
        }

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
    }

    // Hàm helper để kiểm tra vị trí:
    private bool IsInsideInventoryPanel(Vector3 screenPosition, Camera camera)
    {
        if (inventoryPanelRect == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            inventoryPanelRect,
            screenPosition,
            camera
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var slot = GetComponentInParent<Slot>();
        var controller = FindObjectOfType<InventoryController>();
        controller.HandleSlotClick(slot, eventData.button);
    }
}