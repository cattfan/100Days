using UnityEngine;
using UnityEngine.EventSystems;

// Slot.cs: Xử lý hành vi thả của vật phẩm
public class Slot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public GameObject currentItem;
    private InventoryController inventoryController;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        GameObject draggedItem = eventData.pointerDrag;
        ItemUI draggedUI = draggedItem.GetComponent<ItemUI>();
        ItemDragHandler dragHandler = draggedItem.GetComponent<ItemDragHandler>();

        if (draggedUI == null || dragHandler == null) return;

        // Cờ hiệu cho ItemDragHandler biết rằng nó đã được thả vào một Slot hợp lệ
        dragHandler.droppedOnValidSlot = true;

        // Lấy thông tin về slot ban đầu từ dragHandler
        Slot originalSlot = dragHandler.originalParent.GetComponent<Slot>();

        if (originalSlot == transform)
        {
            // Trường hợp 1: Thả vật phẩm trở lại ô ban đầu
            draggedItem.transform.SetParent(transform);
            draggedItem.transform.localPosition = Vector3.zero;
            currentItem = draggedItem;
            return;
        }

        // --- Trường hợp Slot đang trống hoặc không có item nào khác ---
        if (currentItem == null)
        {
            draggedItem.transform.SetParent(transform);
            draggedItem.transform.localPosition = Vector3.zero;
            currentItem = draggedItem;

            if (originalSlot != null)
                originalSlot.currentItem = null;
        }
        else
        {
            // Slot đã có item
            ItemUI existingUI = currentItem.GetComponent<ItemUI>();

            // --- Trường hợp 2: Gộp vật phẩm ---
            if (existingUI.GetItemData() == draggedUI.GetItemData()
                && existingUI.GetItemData().isStackable)
            {
                existingUI.AddAmount(draggedUI.Amount);
                Destroy(draggedItem);

                if (originalSlot != null)
                    originalSlot.currentItem = null;
            }
            else
            {
                // --- Trường hợp 3: Hoán đổi ---
                GameObject existingItem = currentItem;

                draggedItem.transform.SetParent(transform);
                draggedItem.transform.localPosition = Vector3.zero;
                currentItem = draggedItem;

                existingItem.transform.SetParent(dragHandler.originalParent);
                existingItem.transform.localPosition = Vector3.zero;

                if (originalSlot != null)
                    originalSlot.currentItem = existingItem;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryController != null)
        {
            inventoryController.HandleSlotClick(this, eventData.button);
        }
    }
}
