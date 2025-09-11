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

        Debug.Log("AddItem: Attempting to add item " + data.name);

        if (data.isStackable)
        {
            foreach (var s in slots)
            {
                if (s.currentItem != null)
                {
                    var ui = s.currentItem.GetComponent<ItemUI>();
                    if (ui != null && ui.GetItemData() == data)
                    {
                        ui.AddAmount(amount);
                        Debug.Log("AddItem: Added to existing stack.");
                        return true;
                    }
                }
            }
        }

        var emptySlot = slots.FirstOrDefault(s => s.currentItem == null);

        if (emptySlot == null)
        {
            Debug.LogWarning("Inventory full!");
            return false;
        }

        if (itemUIPrefab == null)
        {
            Debug.LogError("AddItem: itemUIPrefab not assigned!");
            return false;
        }

        var itemGO = Instantiate(itemUIPrefab, emptySlot.transform);
        var rt = itemGO.GetComponent<RectTransform>();
        if (rt != null) { rt.anchoredPosition = Vector2.zero; rt.localScale = Vector3.one; }

        var uiComp = itemGO.GetComponent<ItemUI>();
        if (!uiComp) { Debug.LogError("itemUIPrefab missing ItemUI!"); return false; }

        uiComp.Setup(data, amount);
        emptySlot.currentItem = itemGO;
        Debug.Log("AddItem: Added new item to empty slot.");
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