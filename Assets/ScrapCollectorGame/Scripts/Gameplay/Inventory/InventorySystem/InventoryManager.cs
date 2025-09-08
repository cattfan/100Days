using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Setup")]
    [SerializeField] public RectTransform inventoryPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private int slotCount = 18;

    [Header("All Available Items")]
    public ItemData[] allItems;

    [Header("Start Items (optional)")]
    public ItemData[] startItems;

    protected List<Slot> slots = new List<Slot>();

    protected virtual void Start()
    {
        BuildSlots();

        if (startItems != null && startItems.Length > 0)
        {
            foreach (var data in startItems)
            {
                if (data != null) AddItem(data, 1);
            }
        }
    }

    protected void BuildSlots()
    {
        foreach (Transform c in inventoryPanel) Destroy(c.gameObject);
        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            var go = Instantiate(slotPrefab, inventoryPanel);
            var slotComponent = go.GetComponent<Slot>();
            if (!slotComponent)
                Debug.LogError("slotPrefab must have a Slot component!");
            slots.Add(slotComponent);
        }
    }

    public virtual bool AddItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;

        Debug.Log($"🔍 Trying to add {amount} x {data.itemName}");

        // ✅ Nếu stackable, tìm slot đang chứa item này trước
        if (data.isStackable)
        {
            foreach (var s in slots)
            {
                if (s.currentItem != null)
                {
                    var ui = s.currentItem.GetComponent<ItemUI>();
                    if (ui != null && ui.GetItemData() == data)
                    {
                        // Kiểm tra xem có thể thêm được không
                        int canAdd = data.maxStackSize - ui.Amount;
                        if (canAdd > 0)
                        {
                            int toAdd = Mathf.Min(amount, canAdd);
                            ui.AddAmount(toAdd);

                            Debug.Log($" Added {toAdd} to existing stack. New amount: {ui.Amount}");

                            // Popup thành công
                            if (ItemPickupUIController.Instance != null)
                            {
                                ItemPickupUIController.Instance.ShowItemPickup($"{toAdd} {data.itemName}", data.itemIcon);
                            }

                            // Nếu còn dư thì tiếp tục tìm slot khác
                            amount -= toAdd;
                            if (amount <= 0) return true;
                        }
                    }
                }
            }
        }

        // ✅ Nếu vẫn còn amount cần thêm, tìm slot trống
        while (amount > 0)
        {
            var emptySlot = slots.FirstOrDefault(s => s.currentItem == null);
            if (emptySlot == null)
            {
                Debug.Log($"❌ Không có slot trống cho {data.itemName}! Remaining amount: {amount}");
                if (ItemPickupUIController.Instance != null)
                {
                    ItemPickupUIController.Instance.ShowWarningPopup("Túi đồ đã đầy!");
                }
                return amount == 0; // Return true nếu đã add được 1 phần
            }

            // Tạo item mới
            var itemGO = Instantiate(itemUIPrefab, emptySlot.transform);
            var uiComp = itemGO.GetComponent<ItemUI>();
            if (!uiComp) return false;

            // Xác định số lượng để thêm vào slot này
            int toAdd = data.isStackable ? Mathf.Min(amount, data.maxStackSize) : 1;

            uiComp.Setup(data, toAdd);
            emptySlot.currentItem = itemGO;

            Debug.Log($"✅ Created new item in empty slot. Amount: {toAdd}");

            // Popup thành công
            if (ItemPickupUIController.Instance != null)
            {
                ItemPickupUIController.Instance.ShowItemPickup($"+{toAdd} {data.itemName}", data.itemIcon);
            }

            amount -= toAdd;

            // Nếu item không stackable thì chỉ thêm 1 lần
            if (!data.isStackable) break;
        }

        Debug.Log($"🎯 AddItem completed. Remaining amount: {amount}");
        return amount <= 0;
    }



    public virtual void RemoveItem(ItemData data, int amount)
    {
        var slotToRemoveFrom = slots.FirstOrDefault(
            s => s.currentItem != null &&
            s.currentItem.GetComponent<ItemUI>().GetItemData() == data);

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

    public List<Slot> GetSlots() => slots;

    public RectTransform GetInventoryPanel() => inventoryPanel;

    protected ItemData FindItemDataByName(string itemName)
    {
        if (allItems != null)
        {
            foreach (var item in allItems)
            {
                if (item != null && item.name == itemName)
                    return item;
            }
        }
        return null;
    }
    public bool IsInventoryFull()
    {
        // Nếu còn ít nhất 1 slot trống thì chưa đầy
        return slots.All(s => s.currentItem != null);
    }
    public bool IsInventoryFullFor(ItemData data, int amount = 1)
    {
        if (data == null) return true;

        int remainingAmount = amount;

        // Nếu là stackable, kiểm tra các slot hiện có
        if (data.isStackable)
        {
            foreach (var s in slots)
            {
                if (s.currentItem != null)
                {
                    var ui = s.currentItem.GetComponent<ItemUI>();
                    if (ui != null && ui.GetItemData() == data)
                    {
                        int canAdd = data.maxStackSize - ui.Amount;
                        remainingAmount -= canAdd;
                        if (remainingAmount <= 0) return false;
                    }
                }
            }
        }

        // Kiểm tra slot trống
        int emptySlots = slots.Count(s => s.currentItem == null);

        if (data.isStackable)
        {
            // Mỗi slot trống có thể chứa maxStackSize
            int canFitInEmptySlots = emptySlots * data.maxStackSize;
            return remainingAmount > canFitInEmptySlots;
        }
        else
        {
            // Item không stackable cần 1 slot trống cho mỗi item
            return remainingAmount > emptySlots;
        }
    }


}
