using System.Collections.Generic;
using UnityEngine;

public class InventoryPersistence : InventoryManager
{
    public List<InventoryItemData> GetInventoryData()
    {
        List<InventoryItemData> inventoryData = new List<InventoryItemData>();

        for (int i = 0; i < GetSlots().Count; i++)
        {
            var slot = GetSlots()[i];
            if (slot.currentItem != null)
            {
                var itemUI = slot.currentItem.GetComponent<ItemUI>();
                if (itemUI != null && itemUI.GetItemData() != null)
                {
                    var inventoryItem = new InventoryItemData(
                        itemUI.GetItemData().name,
                        itemUI.Amount,
                        i
                    );
                    inventoryData.Add(inventoryItem);
                }
            }
        }
        return inventoryData;
    }

    public void LoadInventoryData(List<InventoryItemData> inventoryData)
    {
        ClearInventory();

        if (inventoryData == null || inventoryData.Count == 0) return;

        foreach (var item in inventoryData)
        {
            ItemData itemData = FindItemDataByName(item.itemName);
            if (itemData != null && item.slotIndex >= 0 && item.slotIndex < GetSlots().Count)
            {
                var targetSlot = GetSlots()[item.slotIndex];
                if (targetSlot.currentItem == null)
                {
                    CreateItemAtSlot(itemData, item.amount, item.slotIndex);
                }
                else
                {
                    AddItem(itemData, item.amount);
                }
            }
        }
    }

    public void ClearInventory()
    {
        foreach (var slot in GetSlots())
        {
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }
    }

    private void CreateItemAtSlot(ItemData data, int amount, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= GetSlots().Count) return;

        var slot = GetSlots()[slotIndex];
        if (slot.currentItem != null) return;

        var itemGO = Instantiate(Resources.Load<GameObject>("ItemUIPrefab"), slot.transform);
        var uiComp = itemGO.GetComponent<ItemUI>();
        if (uiComp != null)
        {
            uiComp.Setup(data, amount);
            slot.currentItem = itemGO;
        }
        else
        {
            Destroy(itemGO);
        }
    }
}
