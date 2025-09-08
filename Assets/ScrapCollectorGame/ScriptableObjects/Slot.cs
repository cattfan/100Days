using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Settings")]
    public GameObject currentItem;

    private InventoryInteraction inventoryInteract;
    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedItem = eventData.pointerDrag;
        if (draggedItem == null) return;

        ItemDragHandler dragHandler = draggedItem.GetComponent<ItemDragHandler>();
        ItemUI draggedUI = draggedItem.GetComponent<ItemUI>();

        if (dragHandler == null || draggedUI == null) return;

        Debug.Log($"[SLOT] Item {draggedItem.name} dropped on slot");

        // Mark as valid drop
        dragHandler.droppedOnValidSlot = true;

        // Get original slot
        Slot originalSlot = dragHandler.originalParent.GetComponent<Slot>();

        // Case 1: Dropping back to same slot
        if (originalSlot == this)
        {
            Debug.Log("[SLOT] Dropping back to same slot");
            ReturnItemToSlot(draggedItem);
            return;
        }

        // Case 2: This slot is empty
        if (currentItem == null)
        {
            Debug.Log("[SLOT] Moving item to empty slot");
            MoveItemToSlot(draggedItem, originalSlot, dragHandler);
        }
        else
        {
            // Case 3: This slot has an item
            ItemUI existingUI = currentItem.GetComponent<ItemUI>();

            // Try to stack if same item type
            if (CanStackItems(existingUI, draggedUI))
            {
                Debug.Log("[SLOT] Stacking items");
                StackItems(existingUI, draggedUI, draggedItem, originalSlot);
            }
            else
            {
                Debug.Log("[SLOT] Swapping items");
                SwapItems(draggedItem, currentItem, originalSlot, dragHandler);
            }
        }
    }

    private void ReturnItemToSlot(GameObject item)
    {
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        currentItem = item;
    }

    private void MoveItemToSlot(GameObject draggedItem, Slot originalSlot, ItemDragHandler dragHandler)
    {
        // Move item to this slot
        draggedItem.transform.SetParent(transform);
        draggedItem.transform.localPosition = Vector3.zero;
        currentItem = draggedItem;

        // Update original parent
        dragHandler.UpdateOriginalParent(transform);

        // Clear original slot
        if (originalSlot != null)
            originalSlot.currentItem = null;
    }

    private bool CanStackItems(ItemUI existingUI, ItemUI draggedUI)
    {
        return existingUI.GetItemData() == draggedUI.GetItemData() &&
               existingUI.GetItemData().isStackable;
    }

    private void StackItems(ItemUI existingUI, ItemUI draggedUI, GameObject draggedItem, Slot originalSlot)
    {
        // Add amounts
        existingUI.AddAmount(draggedUI.Amount);

        // Destroy dragged item
        Destroy(draggedItem);

        // Clear original slot
        if (originalSlot != null)
            originalSlot.currentItem = null;
    }

    private void SwapItems(GameObject draggedItem, GameObject existingItem, Slot originalSlot, ItemDragHandler dragHandler)
    {
        // Place dragged item in this slot
        draggedItem.transform.SetParent(transform);
        draggedItem.transform.localPosition = Vector3.zero;
        currentItem = draggedItem;
        dragHandler.UpdateOriginalParent(transform);

        // Move existing item to original slot
        if (originalSlot != null)
        {
            existingItem.transform.SetParent(originalSlot.transform);
            existingItem.transform.localPosition = Vector3.zero;
            originalSlot.currentItem = existingItem;

            // Update existing item's drag handler
            ItemDragHandler existingHandler = existingItem.GetComponent<ItemDragHandler>();
            if (existingHandler != null)
                existingHandler.UpdateOriginalParent(originalSlot.transform);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryInteract != null)
        {
            inventoryInteract.HandleSlotClick(this, eventData.button);
        }
    }

    // Helper methods
    public bool IsEmpty() => currentItem == null;

    public void ClearSlot()
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }
    }
}