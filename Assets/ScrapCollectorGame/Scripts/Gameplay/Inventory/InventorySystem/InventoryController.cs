using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
    private ItemUI cursorItem;
    private float lastClickTime = 0f;

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
    private void Update()
    {
        if (cursorItem != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                inventoryPanel.root as RectTransform, // root canvas
                Mouse.current.position.ReadValue(),
                null,
                out pos
            );
            cursorItem.GetComponent<RectTransform>().anchoredPosition = pos;
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

    private void HandleLeftClick(Slot slot)
    {
        if (cursorItem == null) // tay trống
        {
            if (slot.currentItem != null)
            {
                cursorItem = slot.currentItem.GetComponent<ItemUI>();
                cursorItem.transform.SetParent(inventoryPanel.root, true);
                slot.currentItem = null;
            }
        }
        else // đang cầm item
        {
            if (slot.currentItem == null)
            {
                slot.currentItem = cursorItem.gameObject;
                cursorItem.transform.SetParent(slot.transform);
                cursorItem.transform.localPosition = Vector3.zero;
                cursorItem = null;
            }
            else
            {
                var slotUI = slot.currentItem.GetComponent<ItemUI>();
                if (slotUI.GetItemData() == cursorItem.GetItemData() && slotUI.GetItemData().isStackable)
                {
                    int moveAmount = cursorItem.Amount;
                    slotUI.AddAmount(moveAmount);
                    Destroy(cursorItem.gameObject);
                    cursorItem = null;
                }
                else
                {
                    // Swap
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
            else
            {
                var slotUI = slot.currentItem.GetComponent<ItemUI>();
                if (slotUI.GetItemData() == cursorItem.GetItemData() && slotUI.GetItemData().isStackable)
                {
                    slotUI.AddAmount(1);
                    cursorItem.AddAmount(-1);
                    if (cursorItem.Amount <= 0) { Destroy(cursorItem.gameObject); cursorItem = null; }
                }
            }
        }
    }
    public RectTransform GetInventoryPanel()
    {
        return inventoryPanel;
    }



}