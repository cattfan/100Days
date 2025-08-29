using UnityEngine;
using UnityEngine.EventSystems;

// Slot.cs: Xử lý hành vi thả của vật phẩm
public class Slot : MonoBehaviour, IDropHandler
{
    public GameObject currentItem;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        GameObject draggedItem = eventData.pointerDrag;
        ItemUI draggedUI = draggedItem.GetComponent<ItemUI>();

        // Cờ hiệu cho ItemUI biết rằng nó đã được thả vào một Slot hợp lệ
        draggedUI.droppedOnValidSlot = true;

        // Lấy thông tin về slot ban đầu
        Slot originalSlot = draggedUI.originalParent.GetComponent<Slot>();

        if (originalSlot == transform)
        {
            // Trường hợp 1: Thả vật phẩm trở lại ô ban đầu
            draggedItem.transform.SetParent(transform);
            draggedItem.transform.localPosition = Vector3.zero;
            return;
        }

        // --- Trường hợp Slot đang trống hoặc không có item nào khác ---
        if (currentItem == null)
        {
            draggedItem.transform.SetParent(transform);
            draggedItem.transform.localPosition = Vector3.zero;
            currentItem = draggedItem;
            // Cập nhật slot ban đầu
            if (originalSlot != null)
            {
                originalSlot.currentItem = null;
            }
        }
        else
        {
            // Trường hợp Slot đã có item
            ItemUI existingUI = currentItem.GetComponent<ItemUI>();

            // --- Trường hợp 2: Gộp vật phẩm ---
            if (existingUI.GetItemData() == draggedUI.GetItemData()
                && existingUI.GetItemData().isStackable)
            {
                existingUI.AddAmount(draggedUI.Amount);
                Destroy(draggedItem);
                // Cập nhật slot ban đầu
                if (originalSlot != null)
                {
                    originalSlot.currentItem = null;
                }
            }
            else
            {
                // --- Trường hợp 3: Hoán đổi vật phẩm ---
                GameObject existingItem = currentItem;

                // Đặt item đang kéo vào slot này
                draggedItem.transform.SetParent(transform);
                draggedItem.transform.localPosition = Vector3.zero;
                currentItem = draggedItem;

                // Trả item cũ về slot ban đầu của item đang kéo
                existingItem.transform.SetParent(draggedUI.originalParent);
                existingItem.transform.localPosition = Vector3.zero;

                // Cập nhật lại reference cho slot ban đầu
                if (originalSlot != null)
                {
                    originalSlot.currentItem = existingItem;
                }
            }
        }
    }
}