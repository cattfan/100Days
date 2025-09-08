using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
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

    private List<Slot> slots = new List<Slot>();
    public bool shopMode = false;

    private void Awake()
    {
        if (!inventoryPanel) Debug.LogError("InventoryController: missing inventoryPanel");
        if (!slotPrefab) Debug.LogError("InventoryController: missing slotPrefab");
        if (!itemUIPrefab) Debug.LogError("InventoryController: missing itemUIPrefab");
    }

    private void Start()
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

    private void BuildSlots()
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

    // SIMPLIFIED CLICK HANDLING - Only for shop mode
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
<<<<<<< HEAD
                else
                {
                    var temp = slot.currentItem;
                    slot.currentItem = cursorItem.gameObject;

                    cursorItem.transform.SetParent(slot.transform);
                    cursorItem.transform.localPosition = Vector3.zero;

                    cursorItem = temp.GetComponent<ItemUI>();
                    cursorItem.transform.SetParent(transform);
                }
            }
        }
    }

    private void HandleRightClick(Slot slot)
    {
        if (cursorItem == null)
        {
            if (slot.currentItem != null)
            {
                var slotUI = slot.currentItem.GetComponent<ItemUI>();
                var itemData = slotUI.GetItemData();

                // Kiểm tra nếu vật phẩm là thức ăn
                if (itemData != null && itemData.isFood)
                {
                    var playerStamina = FindAnyObjectByType<PlayerStamina>();
                    if (playerStamina != null)
                    {
                        playerStamina.RestoreStamina(itemData.staminaRestoreAmount);
                    }

                    // Giảm số lượng vật phẩm đi 1
                    slotUI.AddAmount(-1);
                    if (slotUI.Amount <= 0)
                    {
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                    }
                    return; // THÊM DÒNG NÀY ĐỂ THOÁT KHỎI PHƯƠNG THỨC SAU KHI SỬ DỤNG
                }

                // Logic tách stack cũ, chỉ chạy nếu không phải là đồ ăn
                if (slotUI.Amount > 1)
                {
                    int half = slotUI.Amount / 2;
                    slotUI.AddAmount(-half);

                    var clone = Instantiate(itemUIPrefab, transform).GetComponent<ItemUI>();
                    clone.Setup(slotUI.GetItemData(), half);
                    cursorItem = clone;
                }
                else
                {
                    cursorItem = slotUI;
                    slot.currentItem = null;
                    cursorItem.transform.SetParent(inventoryPanel.root, true);
                }
            }
        }
        else
        {
            if (slot.currentItem == null)
            {
                var clone = Instantiate(itemUIPrefab, slot.transform).GetComponent<ItemUI>();
                clone.Setup(cursorItem.GetItemData(), 1);
                slot.currentItem = clone.gameObject;

                cursorItem.AddAmount(-1);
                if (cursorItem.Amount <= 0) { Destroy(cursorItem.gameObject); cursorItem = null; }
=======
>>>>>>> origin/main
            }
        }
    }

    public List<Slot> GetSlots()
    {
        return slots;
    }

    public RectTransform GetInventoryPanel()
    {
        return inventoryPanel;
    }

    public void OnSlotClicked(Slot slot)
    {
        if (slot == null || slot.currentItem == null) return;

        var itemUI = slot.currentItem.GetComponent<ItemUI>();
        if (itemUI == null) return;

        if (shopMode)
        {
            // Đã thay thế FindFirstObjectByType bằng FindAnyObjectByType
            var shopUI = FindAnyObjectByType<ShopUIManager>();
            if (shopUI != null)
            {
                int slotIndex = GetSlots().IndexOf(slot);
                shopUI.ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
            }
        }
    }

    // SAVE/LOAD METHODS
    public List<InventoryItemData> GetInventoryData()
    {
        List<InventoryItemData> inventoryData = new List<InventoryItemData>();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
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

        Debug.Log($"GetInventoryData: Found {inventoryData.Count} items");
        return inventoryData;
    }

    public void LoadInventoryData(List<InventoryItemData> inventoryData)
    {
        ClearInventory();

        if (inventoryData == null || inventoryData.Count == 0)
        {
            Debug.Log("No inventory data to load");
            return;
        }

        foreach (var item in inventoryData)
        {
            ItemData itemData = FindItemDataByName(item.itemName);
            if (itemData != null && item.slotIndex >= 0 && item.slotIndex < slots.Count)
            {
                var targetSlot = slots[item.slotIndex];
                if (targetSlot.currentItem == null)
                {
                    CreateItemAtSlot(itemData, item.amount, item.slotIndex);
                }
                else
                {
                    AddItem(itemData, item.amount);
                }
            }
            else
            {
                Debug.LogWarning($"Could not load item: {item.itemName}");
            }
        }

        Debug.Log($"LoadInventoryData: Loaded {inventoryData.Count} items");
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
<<<<<<< HEAD
            // Always treat as Left click
            HandleSlotClick(slot, PointerEventData.InputButton.Left);
=======
            Destroy(itemGO);
>>>>>>> origin/main
        }
    }

    private ItemData FindItemDataByName(string itemName)
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