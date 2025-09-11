using UnityEngine;
using UnityEngine.EventSystems;

public partial class Inventory
{
    public bool shopMode = false;

    public void HandleSlotClick(Slot slot, PointerEventData.InputButton button)
    {
        if (shopMode && slot.currentItem != null)
        {
            var itemUI = slot.currentItem.GetComponent<ItemUI>();
            var shopUI = FindFirstObjectByType<ShopUIManager>();
            if (itemUI != null && shopUI != null)
            {
                int slotIndex = GetSlots().IndexOf(slot);
                shopUI.ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
            }
        }
    }

    public void OnSlotClicked(Slot slot)
    {
        if (slot == null || slot.currentItem == null) return;

        var itemUI = slot.currentItem.GetComponent<ItemUI>();
        var shopUI = FindFirstObjectByType<ShopUIManager>();
        if (itemUI != null && shopUI != null && shopMode)
        {
            int slotIndex = GetSlots().IndexOf(slot);
            shopUI.ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
        }
    }
}
