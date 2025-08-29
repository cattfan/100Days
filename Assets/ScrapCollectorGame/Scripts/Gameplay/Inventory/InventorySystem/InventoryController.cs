using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

// InventoryController.cs: Quản lý logic toàn bộ túi đồ
public class InventoryController : MonoBehaviour
{
    [Header("Inventory Setup")]
    [SerializeField] private RectTransform inventoryPanel; // Panel UI có GridLayoutGroup
    [SerializeField] private GameObject slotPrefab;       // Prefab UI Slot (có script Slot)
    [SerializeField] private GameObject itemUIPrefab;     // Prefab UI Item (có script ItemUI)
    [SerializeField] private int slotCount = 18;

    [Header("Start Items (optional)")]
    public ItemData[] startItems; // Kéo các ItemData bạn muốn thấy ngay khi chạy

    // Thêm danh sách này để quản lý dữ liệu vật phẩm thực sự
    private List<ItemData> inventoryData = new List<ItemData>();
    private List<Slot> slots = new List<Slot>();

    private void Awake()
    {
        if (!inventoryPanel) Debug.LogError("InventoryController: missing inventoryPanel");
        if (!slotPrefab) Debug.LogError("InventoryController: missing slotPrefab");
        if (!itemUIPrefab) Debug.LogError("InventoryController: missing itemUIPrefab");
    }

    private void Start()
    {
        BuildSlots();

        // Đổ sẵn các item trong danh sách khởi tạo
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
        // Xoá con cũ và danh sách cũ
        foreach (Transform c in inventoryPanel) Destroy(c.gameObject);
        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            var go = Instantiate(slotPrefab, inventoryPanel);
            var slotComponent = go.GetComponent<Slot>();
            if (!slotComponent)
                Debug.LogError("slotPrefab phải có component Slot!");
            slots.Add(slotComponent);
        }
    }

    public bool AddItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;

        // Nếu stackable → cộng vào slot đang có cùng ItemData
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
                        return true;
                    }
                }
            }
        }

        // Tìm slot trống
        var emptySlot = slots.FirstOrDefault(s => s.currentItem == null);

        if (emptySlot == null)
        {
            Debug.LogWarning("Inventory full!");
            return false;
        }

        // Tạo UI item
        var itemGO = Instantiate(itemUIPrefab, emptySlot.transform);
        var rt = itemGO.GetComponent<RectTransform>();
        if (rt != null) { rt.anchoredPosition = Vector2.zero; rt.localScale = Vector3.one; }

        var uiComp = itemGO.GetComponent<ItemUI>();
        if (!uiComp) { Debug.LogError("itemUIPrefab missing ItemUI!"); return false; }

        uiComp.Setup(data, amount);
        emptySlot.currentItem = itemGO;

        // Thêm dữ liệu vào danh sách
        inventoryData.Add(data);
        return true;
    }
}