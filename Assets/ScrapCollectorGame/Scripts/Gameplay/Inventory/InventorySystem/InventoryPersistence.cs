using System.Collections.Generic;
using UnityEngine;

public class InventoryPersistence : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;

    public List<InventoryItemData> GetInventoryData()
    {
        List<InventoryItemData> inventoryData = new List<InventoryItemData>();
        var slots = inventoryManager.GetSlots();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot.currentItem != null)
            {
                var itemUI = slot.currentItem.GetComponent<ItemUI>();
                if (itemUI != null && itemUI.GetItemData() != null)
                {
                    inventoryData.Add(new InventoryItemData(
                        itemUI.GetItemData().name,
                        itemUI.Amount,
                        i
                    ));
                }
            }
        }
        return inventoryData;
    }

    public void LoadInventoryData(List<InventoryItemData> inventoryData)
    {
        inventoryManager.ClearInventory();
        if (inventoryData == null || inventoryData.Count == 0) return;

        foreach (var item in inventoryData)
        {
            ItemData itemData = inventoryManager.FindItemDataByName(item.itemName);
            if (itemData != null && item.slotIndex >= 0 && item.slotIndex < inventoryManager.GetSlots().Count)
            {
                var targetSlot = inventoryManager.GetSlots()[item.slotIndex];
                if (targetSlot.currentItem == null)
                {
                    inventoryManager.CreateItemAtSlot(itemData, item.amount, item.slotIndex);
                }
                else
                {
                    inventoryManager.AddItem(itemData, item.amount);
                }
            }
        }
    }
}
