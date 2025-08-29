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
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    public bool droppedOnValidSlot = false; // Flag mới: true nếu item được thả vào một Slot hợp lệ

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            Debug.LogWarning("ItemUI: Canvas cha không được tìm thấy. Đảm bảo ItemUI là con của một Canvas.");
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
        if (itemData != null && itemData.isStackable)
            amountText.text = amount > 1 ? amount.ToString() : "";
        else
            amountText.text = "";
    }

    // ---- Drag handlers ----
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Gán lại cờ droppedOnValidSlot mỗi khi bắt đầu kéo
        droppedOnValidSlot = false;
        originalParent = transform.parent;

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
        // Chỉ xử lý nếu item KHÔNG được thả vào một slot hợp lệ
        if (!droppedOnValidSlot)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
    }
}