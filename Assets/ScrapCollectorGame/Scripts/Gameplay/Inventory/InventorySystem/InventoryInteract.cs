using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryInteraction : InventoryManager
{
    [Header("Shop Mode")]
    public bool shopMode = false;

    public void HandleSlotClick(Slot slot, PointerEventData.InputButton button)
    {
        if (shopMode && slot.currentItem != null)
        {
            var itemUI = slot.currentItem.GetComponent<ItemUI>();
            if (itemUI != null)
            {
                var shopUI = FindFirstObjectByType<ShopUIManager>();
                if (shopUI != null)
                {
                    int slotIndex = GetSlots().IndexOf(slot);
                    shopUI.ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
                }
            }
        }
    }

    public void OnSlotClicked(Slot slot)
    {
        if (slot == null || slot.currentItem == null) return;

        var itemUI = slot.currentItem.GetComponent<ItemUI>();
        if (itemUI == null) return;

        if (shopMode)
        {
            var shopUI = FindFirstObjectByType<ShopUIManager>();
            if (shopUI != null)
            {
                int slotIndex = GetSlots().IndexOf(slot);
                shopUI.ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
            }
        }
    }
}
