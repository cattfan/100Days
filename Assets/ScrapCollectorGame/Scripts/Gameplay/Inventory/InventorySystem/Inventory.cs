using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public partial class Inventory
{
    public bool AddItem(ItemData data, int amount = 1)
    {
        if (data == null)
        {
            Debug.LogError("AddItem: Item data is null!");
            return false;
        }

        int remaining = amount;

        if (data.isStackable)
        {
            // 1️⃣ Tìm các stack hiện có và cộng thêm nhưng không vượt maxStackSize
            foreach (var s in slots)
            {
                if (s.currentItem == null) continue;

                var ui = s.currentItem.GetComponent<ItemUI>();
                if (ui != null && ui.GetItemData() == data)
                {
                    int canAdd = Mathf.Min(remaining, data.maxStackSize - ui.Amount);
                    if (canAdd > 0)
                    {
                        ui.AddAmount(canAdd);
                        remaining -= canAdd;
                    }

                    if (remaining <= 0) return true; // đã thêm hết
                }
            }
        }

        // 2️⃣ Nếu còn dư, tạo stack mới cho từng phần còn lại
        while (remaining > 0)
        {
            var emptySlot = slots.FirstOrDefault(s => s.currentItem == null);
            if (emptySlot == null)
            {
                Debug.LogWarning("Inventory full!");
                return false;
            }

            int addAmount = data.isStackable
                ? Mathf.Min(remaining, data.maxStackSize)
                : 1;

            var itemGO = Instantiate(itemUIPrefab, emptySlot.transform);
            var rt = itemGO.GetComponent<RectTransform>();
            if (rt != null) { rt.anchoredPosition = Vector2.zero; rt.localScale = Vector3.one; }

            var uiComp = itemGO.GetComponent<ItemUI>();
            if (!uiComp)
            {
                Debug.LogError("itemUIPrefab missing ItemUI!");
                Destroy(itemGO);
                return false;
            }

            uiComp.Setup(data, addAmount);
            emptySlot.currentItem = itemGO;

            remaining -= addAmount;
        }

        return true;
    }


    public void RemoveItem(ItemData data, int amount)
    {
        var slotToRemoveFrom = slots.FirstOrDefault(s => s.currentItem != null && s.currentItem.GetComponent<ItemUI>().GetItemData() == data);

        if (slotToRemoveFrom != null)
        {
            var itemUI = slotToRemoveFrom.currentItem.GetComponent<ItemUI>();
            itemUI.AddAmount(-amount);

            if (itemUI.Amount <= 0)
            {
                Destroy(slotToRemoveFrom.currentItem);
                slotToRemoveFrom.currentItem = null;
            }
        }
    }

    public void RemoveItemAtSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            var slot = slots[index];
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }
    }
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }
        Debug.Log("Inventory cleared");
    }

    private void CreateItemAtSlot(ItemData data, int amount, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        var slot = slots[slotIndex];
        if (slot.currentItem != null) return;

        var itemGO = Instantiate(itemUIPrefab, slot.transform);
        var rt = itemGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

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
    public int GetTotalItemCount()
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.currentItem != null)
            {
                var itemUI = slot.currentItem.GetComponent<ItemUI>();
                if (itemUI != null)
                {
                    count += itemUI.Amount;
                }
            }
        }
        return count;
    }

    public bool HasItem(string itemName)
    {
        foreach (var slot in slots)
        {
            if (slot.currentItem != null)
            {
                var itemUI = slot.currentItem.GetComponent<ItemUI>();
                if (itemUI != null && itemUI.GetItemData() != null &&
                    itemUI.GetItemData().name == itemName)
                {
                    return true;
                }
            }
        }
        return false;
    }
}