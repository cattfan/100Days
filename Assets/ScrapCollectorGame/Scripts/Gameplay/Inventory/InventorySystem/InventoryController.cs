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
    private Slot originalSlot;
    private ItemUI cursorItem;
    private float lastClickTime = 0f;
    public bool shopMode = false; // <- flag shop mode

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
    private void Update()
    {
        if (cursorItem != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                inventoryPanel.root as RectTransform,
                Mouse.current.position.ReadValue(),
                null,
                out pos
            );
            cursorItem.GetComponent<RectTransform>().anchoredPosition = pos;
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
        // Find the first slot with the item
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

    // New public method to remove an item by slot index
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

    public void HandleSlotClick(Slot slot, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(slot);
        }
        else if (button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(slot);
        }
    }

    // Thay thế method HandleLeftClick cũ bằng cái này:

    private void HandleLeftClick(Slot slot)
    {
        if (cursorItem == null)
        {
            // Bắt đầu kéo item
            if (slot.currentItem != null)
            {
                originalSlot = slot; // Lưu slot gốc
                cursorItem = slot.currentItem.GetComponent<ItemUI>();
                cursorItem.transform.SetParent(inventoryPanel.root, true);
                slot.currentItem = null;
            }
        }
        else
        {
            // Đang kéo item, thả vào slot
            if (slot.currentItem == null)
            {
                // Slot trống - thả vào
                slot.currentItem = cursorItem.gameObject;
                cursorItem.transform.SetParent(slot.transform);
                cursorItem.transform.localPosition = Vector3.zero;
                cursorItem.transform.localScale = Vector3.one;
                cursorItem = null;
                originalSlot = null; // Reset slot gốc
            }
            else
            {
                // Slot có item
                var slotUI = slot.currentItem.GetComponent<ItemUI>();
                if (slotUI.GetItemData() == cursorItem.GetItemData() && slotUI.GetItemData().isStackable)
                {
                    // Stack items cùng loại
                    int moveAmount = cursorItem.Amount;
                    slotUI.AddAmount(moveAmount);
                    Destroy(cursorItem.gameObject);
                    cursorItem = null;
                    originalSlot = null; // Reset slot gốc
                }
                else
                {
                    // Swap items
                    var temp = slot.currentItem;
                    slot.currentItem = cursorItem.gameObject;

                    cursorItem.transform.SetParent(slot.transform);
                    cursorItem.transform.localPosition = Vector3.zero;
                    cursorItem.transform.localScale = Vector3.one;

                    cursorItem = temp.GetComponent<ItemUI>();
                    cursorItem.transform.SetParent(inventoryPanel.root, true);

                    // Cập nhật slot gốc thành slot hiện tại
                    originalSlot = slot;
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
            var shopUI = FindFirstObjectByType<ShopUIManager>();
            if (shopUI != null)
            {
                int slotIndex = GetSlots().IndexOf(slot);
                shopUI.ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
            }
        }
        else
        {
            // luôn coi như Left click
            HandleSlotClick(slot, PointerEventData.InputButton.Left);
        }
    }
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
                        itemUI.GetItemData().name,  // Sử dụng name của ItemData
                        itemUI.Amount,              // Số lượng
                        i                          // Index của slot
                    );
                    inventoryData.Add(inventoryItem);
                }
            }
        }

        Debug.Log($"GetInventoryData: Found {inventoryData.Count} items");
        return inventoryData;
    }

    // Method để load inventory data
    public void LoadInventoryData(List<InventoryItemData> inventoryData)
    {
        // Clear inventory trước khi load
        ClearInventory();

        if (inventoryData == null || inventoryData.Count == 0)
        {
            Debug.Log("No inventory data to load");
            return;
        }

        foreach (var item in inventoryData)
        {
            // Tìm ItemData bằng name
            ItemData itemData = FindItemDataByName(item.itemName);
            if (itemData != null && item.slotIndex >= 0 && item.slotIndex < slots.Count)
            {
                // Đảm bảo slot trống
                var targetSlot = slots[item.slotIndex];
                if (targetSlot.currentItem == null)
                {
                    CreateItemAtSlot(itemData, item.amount, item.slotIndex);
                }
                else
                {
                    // Nếu slot đã có item, tìm slot trống khác
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

    // Method để xóa toàn bộ inventory
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

    // Method để tạo item tại slot cụ thể
    private void CreateItemAtSlot(ItemData data, int amount, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        var slot = slots[slotIndex];
        if (slot.currentItem != null) return; // Slot đã có item

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

    // Method để tìm ItemData bằng name
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

    // Method để đếm tổng số item trong inventory
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

    // Method để kiểm tra có item cụ thể không
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
    // Thêm các method này vào InventoryController class

    // Kiểm tra có item đang được kéo không
    public bool HasCursorItem()
    {
        return cursorItem != null;
    }

    // Trả item về slot trống hoặc slot gốc
    public void ReturnCursorItemToSlot()
    {
        if (cursorItem == null) return;

        // Tìm slot trống đầu tiên
        var emptySlot = slots.FirstOrDefault(s => s.currentItem == null);

        if (emptySlot != null)
        {
            // Đặt vào slot trống
            emptySlot.currentItem = cursorItem.gameObject;
            cursorItem.transform.SetParent(emptySlot.transform);
            cursorItem.transform.localPosition = Vector3.zero;
            cursorItem = null;
            Debug.Log("Returned cursor item to empty slot");
        }
        else
        {
            // Nếu không có slot trống, thử stack với item cùng loại
            bool stackedSuccessfully = false;
            var cursorData = cursorItem.GetItemData();

            if (cursorData.isStackable)
            {
                foreach (var slot in slots)
                {
                    if (slot.currentItem != null)
                    {
                        var slotItemUI = slot.currentItem.GetComponent<ItemUI>();
                        if (slotItemUI != null && slotItemUI.GetItemData() == cursorData)
                        {
                            // Stack với item cùng loại
                            slotItemUI.AddAmount(cursorItem.Amount);
                            Destroy(cursorItem.gameObject);
                            cursorItem = null;
                            stackedSuccessfully = true;
                            Debug.Log("Stacked cursor item with existing item");
                            break;
                        }
                    }
                }
            }

            // Nếu không thể stack, drop item (tùy chọn)
            if (!stackedSuccessfully)
            {
                Debug.LogWarning("Could not return cursor item - inventory full!");
                // Tùy chọn: có thể drop item ra ngoài thế giới game
                // hoặc hiển thị thông báo cho người chơi
                Destroy(cursorItem.gameObject);
                cursorItem = null;
            }
        }
    }

    // Method để force cancel drag (nếu cần)
    public void CancelDrag()
    {
        if (cursorItem != null)
        {
            ReturnCursorItemToSlot();
        }
    }
}